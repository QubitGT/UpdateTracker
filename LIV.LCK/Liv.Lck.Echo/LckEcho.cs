using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AOT;
using Liv.Lck.Core;
using Liv.Lck.Encoding;
using Liv.Lck.Recorder;
using Liv.Lck.Settings;
using Liv.Lck.Telemetry;
using Liv.Lck.Utilities;
using UnityEngine;
using UnityEngine.Scripting;

namespace Liv.Lck.Echo;

internal class LckEcho : ILckEcho, IDisposable, ILckCaptureStateProvider
{
	private readonly ILckEncoder _encoder;

	private readonly ILckOutputConfigurer _outputConfigurer;

	private readonly ILckEventBus _eventBus;

	private readonly ILckTelemetryClient _telemetryClient;

	private readonly ILckStorageWatcher _storageWatcher;

	private IntPtr _echoContext = IntPtr.Zero;

	private LckEncodedPacketHandler _echoPacketHandler;

	private bool _isSaving;

	private bool _disposed;

	private TimeSpan _lastSaveDuration;

	private Dictionary<string, object> _echoTelemetryContext = new Dictionary<string, object>();

	private readonly WaitForSeconds _copyEchoSpinWait = new WaitForSeconds(0.1f);

	private static LckNativeEchoApi.EchoCompletionCallback _completionCallbackDelegate;

	private static LckEcho _activeInstance;

	public bool IsEnabled
	{
		get
		{
			if (_echoContext != IntPtr.Zero)
			{
				return LckNativeEchoApi.IsEchoBufferEnabled(_echoContext);
			}
			return false;
		}
	}

	public bool IsSaving => _isSaving;

	public LckCaptureState CurrentCaptureState
	{
		get
		{
			if (!IsEnabled)
			{
				return LckCaptureState.Idle;
			}
			return LckCaptureState.InProgress;
		}
	}

	[Preserve]
	public LckEcho(ILckEncoder encoder, ILckOutputConfigurer outputConfigurer, ILckEventBus eventBus, ILckTelemetryClient telemetryClient, ILckStorageWatcher storageWatcher)
	{
		_encoder = encoder;
		_outputConfigurer = outputConfigurer;
		_eventBus = eventBus;
		_telemetryClient = telemetryClient;
		_storageWatcher = storageWatcher;
		_eventBus.AddListener<LckEvents.EncoderStoppedEvent>(OnEncoderStopped);
		_eventBus.AddListener<LckEvents.CaptureErrorEvent>(OnCaptureError);
		_eventBus.AddListener<LckEvents.LowStorageSpaceDetectedEvent>(OnLowStorageSpaceDetected);
	}

	public LckResult<bool> IsPaused()
	{
		return LckResult<bool>.NewSuccess(result: false);
	}

	public async Task<LckResult> SetEnabledAsync(bool enabled)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Echo service has been disposed");
		}
		try
		{
			if (enabled)
			{
				return Enable();
			}
			return await DisableAsync();
		}
		catch (Exception ex)
		{
			LckLog.LogError("Echo " + (enabled ? "enable" : "disable") + " failed: " + ex.Message, "SetEnabledAsync", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Echo\\LckEcho.cs", 102);
			DestroyNativeContext();
			return LckResult.NewError(LckError.EncodingError, "Echo " + (enabled ? "enable" : "disable") + " failed: " + ex.Message);
		}
	}

	public LckResult TriggerSave()
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Echo service has been disposed");
		}
		if (!IsEnabled)
		{
			return LckResult.NewError(LckError.RecordingError, "Echo is not enabled");
		}
		if (_isSaving)
		{
			return LckResult.NewError(LckError.CaptureAlreadyStarted, "Echo save already in progress");
		}
		if (!_storageWatcher.HasEnoughFreeStorage())
		{
			return LckResult.NewError(LckError.NotEnoughStorageSpace, "Not enough storage space to save echo.");
		}
		string path = FileUtility.GenerateEchoFilename("mp4");
		string text = Path.Combine(Application.temporaryCachePath, path);
		_lastSaveDuration = GetBufferDuration();
		_isSaving = true;
		if (!LckNativeEchoApi.TriggerEchoSave(_echoContext, text))
		{
			_isSaving = false;
			return LckResult.NewError(LckError.RecordingError, "Failed to trigger echo save - buffer may be empty or missing keyframe");
		}
		LckLog.Log("Echo save triggered to: " + text, "TriggerSave", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Echo\\LckEcho.cs", 136);
		return LckResult.NewSuccess();
	}

	public TimeSpan GetBufferDuration()
	{
		if (_echoContext == IntPtr.Zero)
		{
			return TimeSpan.Zero;
		}
		return TimeSpan.FromMilliseconds((double)LckNativeEchoApi.GetEchoBufferDurationUs(_echoContext) / 1000.0);
	}

	public TimeSpan GetMaxBufferDuration()
	{
		return TimeSpan.FromMilliseconds((double)LckNativeEchoApi.GetEchoBufferMaxDuration(_echoContext) / 1000.0);
	}

	private LckResult Enable()
	{
		if (IsEnabled)
		{
			return LckResult.NewSuccess();
		}
		if (!_storageWatcher.HasEnoughFreeStorage())
		{
			return LckResult.NewError(LckError.NotEnoughStorageSpace, "Not enough storage space to enable echo.");
		}
		_echoContext = LckNativeEchoApi.CreateEchoDiskBuffer(Application.temporaryCachePath);
		if (_echoContext == IntPtr.Zero)
		{
			return LckResult.NewError(LckError.EncodingError, "Failed to create native echo buffer");
		}
		_activeInstance = this;
		_completionCallbackDelegate = OnNativeEchoCompleted;
		LckNativeEchoApi.SetEchoCompletionCallback(_echoContext, _completionCallbackDelegate);
		LckResult<MuxerConfig> lckResult = BuildMuxerConfig();
		if (!lckResult.Success)
		{
			DestroyNativeContext();
			return LckResult.NewError(LckError.EncodingError, lckResult.Message);
		}
		MuxerConfig config = lckResult.Result;
		LckNativeEchoApi.SetEchoMuxerConfig(_echoContext, ref config);
		LckEncodedPacketCallback encodedPacketCallback = new LckEncodedPacketCallback(_echoContext, LckNativeEchoApi.GetEchoCallbackFunction());
		_echoPacketHandler = new LckEncodedPacketHandler(this, encodedPacketCallback);
		LckResult<CameraTrackDescriptor> activeCameraTrackDescriptor = _outputConfigurer.GetActiveCameraTrackDescriptor();
		if (!activeCameraTrackDescriptor.Success)
		{
			DestroyNativeContext();
			return LckResult.NewError(LckError.EncodingError, activeCameraTrackDescriptor.Message);
		}
		LckResult lckResult2 = _encoder.AcquireEncoder(EncoderConsumer.Echo, activeCameraTrackDescriptor.Result, new LckEncodedPacketHandler[1] { _echoPacketHandler });
		if (!lckResult2.Success)
		{
			DestroyNativeContext();
			return lckResult2;
		}
		LckNativeEchoApi.SetEchoBufferEnabled(_echoContext, enabled: true);
		LckLog.Log("Echo enabled", "Enable", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Echo\\LckEcho.cs", 214);
		_echoTelemetryContext = new Dictionary<string, object>
		{
			{
				"echo.targetResolutionX",
				activeCameraTrackDescriptor.Result.CameraResolutionDescriptor.Width
			},
			{
				"echo.targetResolutionY",
				activeCameraTrackDescriptor.Result.CameraResolutionDescriptor.Height
			},
			{
				"echo.targetFramerate",
				activeCameraTrackDescriptor.Result.Framerate
			},
			{
				"echo.targetBitrate",
				activeCameraTrackDescriptor.Result.Bitrate
			},
			{
				"echo.targetAudioBitrate",
				activeCameraTrackDescriptor.Result.AudioBitrate
			},
			{
				"echo.bufferDuration",
				(double)LckNativeEchoApi.GetEchoBufferMaxDuration(_echoContext) / 1000000.0
			}
		};
		_telemetryClient.SendTelemetry(new LckTelemetryEvent(LckTelemetryEventType.EchoEnabled, _echoTelemetryContext));
		_eventBus.Trigger(new LckEvents.EchoEnabledEvent(LckResult.NewSuccess()));
		return LckResult.NewSuccess();
	}

	private async Task<LckResult> DisableAsync()
	{
		if (!IsEnabled)
		{
			_eventBus.Trigger(new LckEvents.EchoDisabledEvent(LckResult.NewSuccess()));
			return LckResult.NewSuccess();
		}
		LckNativeEchoApi.SetEchoBufferEnabled(_echoContext, enabled: false);
		IntPtr echoContext = _echoContext;
		_echoContext = IntPtr.Zero;
		_activeInstance = null;
		await _encoder.ReleaseEncoderAsync(EncoderConsumer.Echo, new LckEncodedPacketHandler[1] { _echoPacketHandler });
		DestroyEchoBuffer(echoContext);
		LckLog.Log("Echo disabled", "DisableAsync", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Echo\\LckEcho.cs", 248);
		_eventBus.Trigger(new LckEvents.EchoDisabledEvent(LckResult.NewSuccess()));
		return LckResult.NewSuccess();
	}

	private void DestroyNativeContext()
	{
		DestroyEchoBuffer(_echoContext);
		_echoContext = IntPtr.Zero;
		_activeInstance = null;
	}

	private static void DestroyEchoBuffer(IntPtr echoContext)
	{
		if (echoContext != IntPtr.Zero)
		{
			LckNativeEchoApi.DestroyEchoBuffer(echoContext);
		}
	}

	private LckResult<MuxerConfig> BuildMuxerConfig()
	{
		LckResult<CameraTrackDescriptor> activeCameraTrackDescriptor = _outputConfigurer.GetActiveCameraTrackDescriptor();
		if (!activeCameraTrackDescriptor.Success)
		{
			return LckResult<MuxerConfig>.NewError(LckError.EncodingError, "Failed to get camera track descriptor");
		}
		LckResult<uint> audioSampleRate = _outputConfigurer.GetAudioSampleRate();
		if (!audioSampleRate.Success)
		{
			return LckResult<MuxerConfig>.NewError(LckError.EncodingError, "Failed to get audio sample rate");
		}
		LckResult<uint> numberOfAudioChannels = _outputConfigurer.GetNumberOfAudioChannels();
		if (!numberOfAudioChannels.Success)
		{
			return LckResult<MuxerConfig>.NewError(LckError.EncodingError, "Failed to get number of audio channels");
		}
		CameraTrackDescriptor result = activeCameraTrackDescriptor.Result;
		MuxerConfig result2 = default(MuxerConfig);
		result2.outputPath = "";
		result2.videoBitrate = result.Bitrate;
		result2.audioBitrate = result.AudioBitrate;
		result2.width = result.CameraResolutionDescriptor.Width;
		result2.height = result.CameraResolutionDescriptor.Height;
		result2.framerate = result.Framerate;
		result2.samplerate = audioSampleRate.Result;
		result2.channels = numberOfAudioChannels.Result;
		result2.numberOfTracks = 2u;
		result2.realtimeOutput = false;
		return LckResult<MuxerConfig>.NewSuccess(result2);
	}

	[MonoPInvokeCallback(typeof(LckNativeEchoApi.EchoCompletionCallback))]
	private static void OnNativeEchoCompleted(uint status, string outputPath)
	{
		LckMonoBehaviourMediator.Instance.EnqueueMainThreadAction(delegate
		{
			LckEcho activeInstance = _activeInstance;
			if (activeInstance != null && !activeInstance._disposed)
			{
				if (status == 0)
				{
					LckLog.Log("Echo save completed: " + outputPath, "OnNativeEchoCompleted", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Echo\\LckEcho.cs", 311);
					LckMonoBehaviourMediator.StartCoroutine("CopyEchoToGalleryWhenReady", activeInstance.CopyEchoToGalleryWhenReady(outputPath));
				}
				else
				{
					activeInstance._isSaving = false;
					string message = $"Echo save failed with status {status}";
					LckLog.LogError(message, "OnNativeEchoCompleted", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Echo\\LckEcho.cs", 319);
					activeInstance._eventBus.Trigger(new LckEvents.EchoSavedEvent(LckResult<RecordingData>.NewError(LckError.RecordingError, message)));
				}
			}
		});
	}

	private IEnumerator CopyEchoToGalleryWhenReady(string outputPath)
	{
		while (FileUtility.IsFileLocked(outputPath) && File.Exists(outputPath))
		{
			yield return _copyEchoSpinWait;
		}
		Task task = FileUtility.CopyToGallery(outputPath, LckSettings.Instance.RecordingAlbumName, delegate(bool success, string path)
		{
			LckMonoBehaviourMediator.Instance.EnqueueMainThreadAction(delegate
			{
				if (!_disposed)
				{
					_isSaving = false;
					if (success)
					{
						LckLog.Log("LCK Echo saved to gallery: " + path, "CopyEchoToGalleryWhenReady", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Echo\\LckEcho.cs", 344);
						RecordingData recordingData = default(RecordingData);
						recordingData.RecordingFilePath = path;
						recordingData.RecordingDuration = (float)_lastSaveDuration.TotalSeconds;
						RecordingData result = recordingData;
						_eventBus.Trigger(new LckEvents.EchoSavedEvent(LckResult<RecordingData>.NewSuccess(result)));
						Dictionary<string, object> dictionary = new Dictionary<string, object>(_echoTelemetryContext) { { "echo.duration", _lastSaveDuration.TotalSeconds } };
						ulong encodedVideoFrames = _encoder.GetCurrentSessionData().EncodedVideoFrames;
						float num = (float)_lastSaveDuration.TotalSeconds;
						float num2 = ((num > 0f && encodedVideoFrames != 0) ? ((float)encodedVideoFrames / num) : 0f);
						dictionary.Add("echo.encodedFrames", encodedVideoFrames);
						dictionary.Add("echo.actualFramerate", num2);
						_telemetryClient.SendTelemetry(new LckTelemetryEvent(LckTelemetryEventType.EchoSaved, dictionary));
					}
					else
					{
						LckLog.LogError("LCK Failed to save echo to gallery", "CopyEchoToGalleryWhenReady", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Echo\\LckEcho.cs", 371);
						_eventBus.Trigger(new LckEvents.EchoSavedEvent(LckResult<RecordingData>.NewError(LckError.FailedToCopyRecordingToGallery, "Failed to copy echo recording to Gallery")));
					}
				}
			});
		});
		yield return new WaitUntil(() => task.IsCompleted);
	}

	private void OnLowStorageSpaceDetected(LckEvents.LowStorageSpaceDetectedEvent lowStorageSpaceDetectedEvent)
	{
		if (IsEnabled)
		{
			LckLog.Log("Low storage space detected - disabling echo", "OnLowStorageSpaceDetected", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Echo\\LckEcho.cs", 388);
			_eventBus.Trigger(new LckEvents.EchoDisabledEvent(LckResult.NewSuccess(), EchoDisableReason.LowStorage));
			DisableAsync();
		}
	}

	private void OnEncoderStopped(LckEvents.EncoderStoppedEvent encoderStoppedEvent)
	{
		if (IsEnabled)
		{
			LckLog.Log("Encoder stopped while echo was active - disabling echo", "OnEncoderStopped", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Echo\\LckEcho.cs", 399);
			DisableAsync();
		}
	}

	private void OnCaptureError(LckEvents.CaptureErrorEvent captureErrorEvent)
	{
		if (IsEnabled)
		{
			LckLog.LogError("Echo capture error: " + captureErrorEvent.Error.Message, "OnCaptureError", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Echo\\LckEcho.cs", 408);
			_eventBus.Trigger(new LckEvents.EchoDisabledEvent(LckResult.NewSuccess(), EchoDisableReason.Error));
			DisableAsync();
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			if (IsEnabled)
			{
				DisableAsync();
			}
			_eventBus.RemoveListener<LckEvents.EncoderStoppedEvent>(OnEncoderStopped);
			_eventBus.RemoveListener<LckEvents.CaptureErrorEvent>(OnCaptureError);
			_eventBus.RemoveListener<LckEvents.LowStorageSpaceDetectedEvent>(OnLowStorageSpaceDetected);
			_disposed = true;
		}
	}
}
