using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Liv.Lck.Core;
using Liv.Lck.Encoding;
using Liv.Lck.Settings;
using Liv.Lck.Telemetry;
using Liv.Lck.Utilities;
using Liv.NGFX;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Scripting;

namespace Liv.Lck.Recorder;

internal class LckRecorder : ILckRecorder, ILckCaptureStateProvider, IDisposable
{
	private readonly ILckNativeRecordingService _nativeRecordingService;

	private readonly ILckStorageWatcher _storageWatcher;

	private readonly ILckEncoder _encoder;

	private readonly ILckOutputConfigurer _outputConfigurer;

	private readonly ILckEventBus _eventBus;

	private readonly ILckTelemetryClient _telemetryClient;

	private readonly ILckTelemetryContextProvider _telemetryContextProvider;

	private static readonly ProfilerMarker _copyOutputFileToNativeGalleryMarker = new ProfilerMarker("LckRecorder.CopyOutputFileToNativeGallery");

	private const EncoderConsumer ConsumerName = EncoderConsumer.Recording;

	private MuxerConfig _muxerConfig;

	private float _recordingStartTime;

	private float _accumulatedRecordingDuration;

	private float _lastActiveSegmentStartTime;

	private string _lastRecordingFilePath;

	private LckService.StopReason _stopReason;

	private CameraTrackDescriptor _currentRecordingDescriptor;

	private LckEncodedPacketHandler _recordingPacketHandler;

	private Dictionary<string, object> _recordingTelemetryContext = new Dictionary<string, object>();

	private bool _disposed;

	private WaitForSeconds _copyVideoSpinWait = new WaitForSeconds(0.1f);

	public LckCaptureState CurrentCaptureState { get; private set; }

	private float ActualRecordingDurationSeconds
	{
		get
		{
			if (CurrentCaptureState == LckCaptureState.InProgress)
			{
				return _accumulatedRecordingDuration + (Time.time - _lastActiveSegmentStartTime);
			}
			return _accumulatedRecordingDuration;
		}
	}

	[Preserve]
	public LckRecorder(ILckNativeRecordingService nativeRecordingService, ILckEncoder encoder, ILckOutputConfigurer outputConfigurer, ILckStorageWatcher storageWatcher, ILckEventBus eventBus, ILckTelemetryClient telemetryClient, ILckTelemetryContextProvider telemetryContextProvider)
	{
		_nativeRecordingService = nativeRecordingService;
		_encoder = encoder;
		_outputConfigurer = outputConfigurer;
		_storageWatcher = storageWatcher;
		_eventBus = eventBus;
		_telemetryClient = telemetryClient;
		_telemetryContextProvider = telemetryContextProvider;
		_eventBus.AddListener<LckEvents.LowStorageSpaceDetectedEvent>(OnLowStorageSpaceDetected);
		_eventBus.AddListener<LckEvents.EncoderStoppedEvent>(OnEncoderStopped);
		_eventBus.AddListener<LckEvents.CaptureErrorEvent>(OnCaptureError);
	}

	public LckResult<bool> IsRecording()
	{
		return LckResult<bool>.NewSuccess(CurrentCaptureState == LckCaptureState.InProgress);
	}

	public LckResult<bool> IsPaused()
	{
		return LckResult<bool>.NewSuccess(CurrentCaptureState == LckCaptureState.Paused);
	}

	public void SetLogLevel(Liv.NGFX.LogLevel logLevel)
	{
		_nativeRecordingService.SetNativeMuxerLogLevel(logLevel);
	}

	public LckResult StartRecording()
	{
		if (CurrentCaptureState != 0)
		{
			return LckResult.NewError(LckError.CaptureAlreadyStarted, "Recording already started.");
		}
		if (!_storageWatcher.HasEnoughFreeStorage())
		{
			return LckResult.NewError(LckError.NotEnoughStorageSpace, "Not enough storage space.");
		}
		StartRecordingProcess();
		return LckResult.NewSuccess();
	}

	public LckResult StopRecording(LckService.StopReason stopReason)
	{
		LckLog.Log(string.Format("LCK {0} triggered with stop reason: {1}", "StopRecording", stopReason), "StopRecording", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckRecorder.cs", 102);
		if (CurrentCaptureState != LckCaptureState.InProgress)
		{
			return LckResult.NewError(LckError.NotCurrentlyRecording, "No recording currently in progress to stop.");
		}
		LckLog.Log($"LCK StopRecording triggered with stopreason: {stopReason}", "StopRecording", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckRecorder.cs", 108);
		_stopReason = stopReason;
		StopRecordingProcess();
		return LckResult.NewSuccess();
	}

	public LckResult PauseRecording()
	{
		LckResult result;
		if (CurrentCaptureState != LckCaptureState.InProgress)
		{
			result = LckResult.NewError(LckError.NotCurrentlyRecording, "Cannot pause because recording is not in progress.");
		}
		else
		{
			_accumulatedRecordingDuration += Time.time - _lastActiveSegmentStartTime;
			result = LckResult.NewSuccess();
			CurrentCaptureState = LckCaptureState.Paused;
			LckLog.Log("LCK Recording paused.", "PauseRecording", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckRecorder.cs", 129);
		}
		TriggerRecordingPausedEvent(result);
		return result;
	}

	public LckResult ResumeRecording()
	{
		LckResult result;
		if (CurrentCaptureState != LckCaptureState.Paused)
		{
			result = LckResult.NewError(LckError.NotPaused, "Cannot resume because recording is not paused.");
		}
		else
		{
			_lastActiveSegmentStartTime = Time.time;
			result = LckResult.NewSuccess();
			CurrentCaptureState = LckCaptureState.InProgress;
			LckLog.Log("LCK Recording resumed.", "ResumeRecording", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckRecorder.cs", 149);
		}
		TriggerRecordingResumedEvent(result);
		return result;
	}

	public LckResult<TimeSpan> GetRecordingDuration()
	{
		if (CurrentCaptureState == LckCaptureState.Idle)
		{
			return LckResult<TimeSpan>.NewError(LckError.NotCurrentlyRecording, "Recording has not been started.");
		}
		return LckResult<TimeSpan>.NewSuccess(TimeSpan.FromSeconds(ActualRecordingDurationSeconds));
	}

	private async Task<LckResult> StartNativeMuxerAsync()
	{
		return await Task.Run(delegate
		{
			try
			{
				return _nativeRecordingService.StartNativeMuxer(ref _muxerConfig) ? LckResult.NewSuccess() : LckResult.NewError(LckError.RecordingError, "Failed to start native muxer");
			}
			catch (Exception ex)
			{
				LckLog.LogError($"Failed to start recording - exception occurred while starting muxer: {ex}", "StartNativeMuxerAsync", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckRecorder.cs", 178);
				CurrentCaptureState = LckCaptureState.Idle;
				return LckResult.NewError(LckError.RecordingError, ex.Message);
			}
		});
	}

	private async Task StartRecordingAsync()
	{
		try
		{
			LckLog.Log("LCK Starting Recording", "StartRecordingAsync", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckRecorder.cs", 189);
			_outputConfigurer.SetActiveCaptureType(LckCaptureType.Recording);
			LckResult<CameraTrackDescriptor> activeCameraTrackDescriptor = _outputConfigurer.GetActiveCameraTrackDescriptor();
			if (!activeCameraTrackDescriptor.Success)
			{
				CurrentCaptureState = LckCaptureState.Idle;
				LckResult result = LckResult.NewError(LckError.UnknownError, activeCameraTrackDescriptor.Message);
				TriggerRecordingStartedEvent(result);
				return;
			}
			_currentRecordingDescriptor = activeCameraTrackDescriptor.Result;
			UpdateRecordingTelemetryContext();
			if (!_nativeRecordingService.CreateNativeMuxer())
			{
				LckResult result2 = LckResult.NewError(LckError.RecordingError, "Failed to create native muxer");
				CurrentCaptureState = LckCaptureState.Idle;
				TriggerRecordingStartedEvent(result2);
				return;
			}
			_muxerConfig = CreateMuxerConfig();
			LckResult lckResult = await StartNativeMuxerAsync();
			if (!lckResult.Success)
			{
				LckLog.LogError("LCK Recording could not be started", "StartRecordingAsync", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckRecorder.cs", 219);
				CurrentCaptureState = LckCaptureState.Idle;
				TriggerRecordingStartedEvent(lckResult);
				return;
			}
			CurrentCaptureState = LckCaptureState.InProgress;
			_recordingStartTime = Time.time;
			_accumulatedRecordingDuration = 0f;
			_lastActiveSegmentStartTime = Time.time;
			_storageWatcher.SetRecordingContext(_currentRecordingDescriptor, () => ActualRecordingDurationSeconds);
			_recordingPacketHandler = new LckEncodedPacketHandler(this, _nativeRecordingService.GetMuxPacketCallback());
			LckResult lckResult2 = _encoder.AcquireEncoder(EncoderConsumer.Recording, _currentRecordingDescriptor, new LckEncodedPacketHandler[1] { _recordingPacketHandler });
			if (!lckResult2.Success)
			{
				CurrentCaptureState = LckCaptureState.Idle;
				TriggerRecordingStartedEvent(lckResult2);
				return;
			}
			TriggerRecordingStartedEvent(LckResult.NewSuccess());
			LckLog.Log("Recording started successfully", "StartRecordingAsync", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckRecorder.cs", 244);
		}
		catch (Exception e)
		{
			LckLog.LogError("LCK Start Recording Task failed: " + e.Message, "StartRecordingAsync", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckRecorder.cs", 248);
			if (_encoder.IsActive())
			{
				await _encoder.ReleaseEncoderAsync(EncoderConsumer.Recording, new LckEncodedPacketHandler[1] { _recordingPacketHandler });
			}
			TriggerRecordingStartedEvent(LckResult.NewError(LckError.RecordingError, e.Message));
		}
	}

	private async Task<LckResult> StopNativeMuxerAsync()
	{
		return await Task.Run(delegate
		{
			try
			{
				return _nativeRecordingService.StopNativeMuxer() ? LckResult.NewSuccess() : LckResult.NewError(LckError.RecordingError, "Failed to stop native muxer");
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				return LckResult.NewError(LckError.RecordingError, ex.Message);
			}
		});
	}

	private async Task StopRecordingAsync()
	{
		_ = 1;
		try
		{
			LckResult lckResult = await _encoder.ReleaseEncoderAsync(EncoderConsumer.Recording, new LckEncodedPacketHandler[1] { _recordingPacketHandler });
			if (!lckResult.Success)
			{
				TriggerRecordingStoppedEvent(lckResult);
				return;
			}
			LckResult lckResult2 = await StopNativeMuxerAsync();
			if (!lckResult2.Success)
			{
				TriggerRecordingStoppedEvent(lckResult2);
				return;
			}
			CurrentCaptureState = LckCaptureState.Idle;
			_storageWatcher.ClearRecordingContext();
			LckMonoBehaviourMediator.StartCoroutine("CopyRecordingToGalleryWhenReady", CopyRecordingToGalleryWhenReady());
			TriggerRecordingStoppedEvent(LckResult.NewSuccess());
			_nativeRecordingService.DestroyNativeMuxer();
		}
		catch (Exception ex)
		{
			LckLog.LogError("LCK Stop Recording failed: " + ex.Message, "StopRecordingAsync", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckRecorder.cs", 317);
			TriggerRecordingStoppedEvent(LckResult.NewError(LckError.RecordingError, ex.Message));
		}
	}

	private IEnumerator CopyRecordingToGalleryWhenReady()
	{
		while (FileUtility.IsFileLocked(_lastRecordingFilePath) && File.Exists(_lastRecordingFilePath))
		{
			yield return _copyVideoSpinWait;
		}
		using (_copyOutputFileToNativeGalleryMarker.Auto())
		{
			Task task = FileUtility.CopyToGallery(_lastRecordingFilePath, LckSettings.Instance.RecordingAlbumName, delegate(bool success, string path)
			{
				LckMonoBehaviourMediator.Instance.EnqueueMainThreadAction(delegate
				{
					if (success)
					{
						LckLog.Log("LCK Recording saved to gallery: " + path, "CopyRecordingToGalleryWhenReady", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckRecorder.cs", 339);
						RecordingData recordingData = default(RecordingData);
						recordingData.RecordingFilePath = path;
						recordingData.RecordingDuration = ActualRecordingDurationSeconds;
						RecordingData result = recordingData;
						TriggerRecordingSavedEvent(LckResult<RecordingData>.NewSuccess(result));
					}
					else
					{
						TriggerRecordingSavedEvent(LckResult<RecordingData>.NewError(LckError.FailedToCopyRecordingToGallery, "Failed to copy recording to Gallery"));
						LckLog.LogError("LCK Failed to save recording to gallery", "CopyRecordingToGalleryWhenReady", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckRecorder.cs", 351);
					}
				});
			});
			yield return new WaitUntil(() => task.IsCompleted);
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			if (CurrentCaptureState == LckCaptureState.InProgress)
			{
				StopRecordingAsync();
			}
			_storageWatcher.ClearRecordingContext();
			_eventBus.RemoveListener<LckEvents.LowStorageSpaceDetectedEvent>(OnLowStorageSpaceDetected);
			_eventBus.RemoveListener<LckEvents.EncoderStoppedEvent>(OnEncoderStopped);
			_eventBus.RemoveListener<LckEvents.CaptureErrorEvent>(OnCaptureError);
			_disposed = true;
		}
	}

	private void StartRecordingProcess()
	{
		CurrentCaptureState = LckCaptureState.Starting;
		StartRecordingAsync();
	}

	private void StopRecordingProcess()
	{
		LckLog.Log("LCK Stopping Recording", "StopRecordingProcess", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckRecorder.cs", 386);
		if (CurrentCaptureState == LckCaptureState.InProgress)
		{
			_accumulatedRecordingDuration += Time.time - _lastActiveSegmentStartTime;
		}
		CurrentCaptureState = LckCaptureState.Stopping;
		SendRecordingStoppedTelemetry();
		StopRecordingAsync();
	}

	private void SendRecordingStoppedTelemetry()
	{
		ulong encodedVideoFrames = _encoder.GetCurrentSessionData().EncodedVideoFrames;
		float actualRecordingDurationSeconds = ActualRecordingDurationSeconds;
		float num = ((actualRecordingDurationSeconds > 0f && encodedVideoFrames != 0) ? ((float)encodedVideoFrames / actualRecordingDurationSeconds) : 0f);
		_recordingTelemetryContext.Add("recording.duration", actualRecordingDurationSeconds);
		_recordingTelemetryContext.Add("recording.encodedFrames", encodedVideoFrames);
		_recordingTelemetryContext.Add("recording.stopReason", _stopReason.ToString());
		_recordingTelemetryContext.Add("recording.actualFramerate", num);
		_telemetryClient.SendTelemetry(new LckTelemetryEvent(LckTelemetryEventType.RecordingStopped, _recordingTelemetryContext));
	}

	private void UpdateRecordingTelemetryContext()
	{
		_recordingTelemetryContext = new Dictionary<string, object>
		{
			{ "recording.targetFramerate", _currentRecordingDescriptor.Framerate },
			{ "recording.targetBitrate", _currentRecordingDescriptor.Bitrate },
			{ "recording.targetAudioBitrate", _currentRecordingDescriptor.AudioBitrate },
			{
				"recording.targetResolutionX",
				_currentRecordingDescriptor.CameraResolutionDescriptor.Width
			},
			{
				"recording.targetResolutionY",
				_currentRecordingDescriptor.CameraResolutionDescriptor.Height
			}
		};
		_telemetryContextProvider.SetTelemetryContext(LckTelemetryContextType.RecordingContext, _recordingTelemetryContext);
	}

	private MuxerConfig CreateMuxerConfig()
	{
		string path = FileUtility.GenerateFilename("mp4");
		_lastRecordingFilePath = Path.Combine(Application.temporaryCachePath, path);
		CameraTrackDescriptor result = _outputConfigurer.GetActiveCameraTrackDescriptor().Result;
		MuxerConfig result2 = default(MuxerConfig);
		result2.outputPath = _lastRecordingFilePath;
		result2.videoBitrate = result.Bitrate;
		result2.audioBitrate = result.AudioBitrate;
		result2.width = result.CameraResolutionDescriptor.Width;
		result2.height = result.CameraResolutionDescriptor.Height;
		result2.framerate = result.Framerate;
		result2.samplerate = _outputConfigurer.GetAudioSampleRate().Result;
		result2.channels = _outputConfigurer.GetNumberOfAudioChannels().Result;
		result2.numberOfTracks = 2u;
		result2.realtimeOutput = false;
		return result2;
	}

	private void OnEncoderStopped(LckEvents.EncoderStoppedEvent encoderStoppedEvent)
	{
		if (CurrentCaptureState == LckCaptureState.InProgress)
		{
			LckLog.LogError("Encoder stopped while recording - stopping recording", "OnEncoderStopped", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckRecorder.cs", 454);
			StopRecording(LckService.StopReason.Error);
		}
	}

	private void OnLowStorageSpaceDetected(LckEvents.LowStorageSpaceDetectedEvent lowStorageSpaceDetectedEvent)
	{
		StopRecording(LckService.StopReason.LowStorageSpace);
	}

	private void TriggerRecordingStartedEvent(LckResult result)
	{
		_eventBus.Trigger(new LckEvents.RecordingStartedEvent(result));
	}

	private void TriggerRecordingPausedEvent(LckResult result)
	{
		_eventBus.Trigger(new LckEvents.RecordingPausedEvent(result));
	}

	private void TriggerRecordingResumedEvent(LckResult result)
	{
		_eventBus.Trigger(new LckEvents.RecordingResumedEvent(result));
	}

	private void TriggerRecordingStoppedEvent(LckResult result)
	{
		_eventBus.Trigger(new LckEvents.RecordingStoppedEvent(result));
	}

	private void TriggerRecordingSavedEvent(LckResult<RecordingData> result)
	{
		_eventBus.Trigger(new LckEvents.RecordingSavedEvent(result));
	}

	private void OnCaptureError(LckEvents.CaptureErrorEvent captureErrorEvent)
	{
		if (CurrentCaptureState != 0 && CurrentCaptureState != LckCaptureState.Stopping)
		{
			LckLog.LogError("Stopping recording because a capture error occurred: " + captureErrorEvent.Error.Message, "OnCaptureError", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckRecorder.cs", 494);
			StopRecording(LckService.StopReason.Error);
		}
	}
}
