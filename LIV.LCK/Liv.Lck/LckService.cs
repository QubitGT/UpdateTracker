using System;
using System.Threading.Tasks;
using Liv.Lck.Core;
using Liv.Lck.DependencyInjection;
using Liv.Lck.Echo;
using Liv.Lck.Encoding;
using Liv.Lck.Recorder;
using Liv.Lck.Settings;
using Liv.Lck.Streaming;
using Liv.Lck.Telemetry;
using Liv.NGFX;
using Liv.NativeAudioBridge;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace Liv.Lck;

[Preserve]
public class LckService : ILckService, IDisposable
{
	public enum StopReason
	{
		UserStopped,
		LowStorageSpace,
		Error,
		ApplicationLifecycle
	}

	private readonly ILckOutputConfigurer _outputConfigurer;

	private ILckEncodeLooper _encodeLooper;

	private INativeAudioPlayer _nativeAudioPlayer;

	private ILckEncoder _encoder;

	private ILckEcho _echo;

	private bool _disposed;

	private ILckRecorder _recorder;

	private ILckStreamer _streamer;

	private ILckPhotoCapture _photoCapture;

	private ILckStorageWatcher _storageWatcher;

	private ILckVideoMixer _videoMixer;

	private ILckAudioMixer _audioMixer;

	private ILckPreviewer _previewer;

	private ILckEventBus _eventBus;

	private ILckVideoCapturer _videoCapturer;

	private readonly ILckTelemetryClient _telemetryClient;

	private readonly LckPublicApiEventBridge _eventBridge;

	private readonly LckEventErrorLogger _eventErrorLogger;

	public event Action<LckResult> OnRecordingStarted;

	public event Action<LckResult> OnRecordingStopped;

	public event Action<LckResult> OnRecordingPaused;

	public event Action<LckResult> OnRecordingResumed;

	public event Action<LckResult> OnStreamingStarted;

	public event Action<LckResult> OnStreamingStopped;

	public event Action<LckResult> OnLowStorageSpace;

	public event Action<LckResult<RecordingData>> OnRecordingSaved;

	public event Action<LckResult> OnPhotoSaved;

	public event Action<LckResult<RecordingData>> OnEchoSaved;

	public event Action<LckResult> OnEchoEnabled;

	public event Action<LckResult, EchoDisableReason> OnEchoDisabled;

	public event Action<LckResult<ILckCamera>> OnActiveCameraSet;

	[Preserve]
	internal LckService(ILckEncoder encoder, ILckRecorder recorder, ILckStreamer streamer, ILckEcho echo, ILckEncodeLooper encodeLooper, ILckPhotoCapture photoCapture, ILckStorageWatcher storageWatcher, ILckVideoCapturer videoCapturer, ILckVideoMixer videoMixer, ILckAudioMixer audioMixer, ILckOutputConfigurer outputConfigurer, ILckPreviewer previewer, INativeAudioPlayer nativeAudioPlayer, ILckEventBus eventBus, ILckTelemetryClient telemetryClient)
	{
		_encodeLooper = encodeLooper;
		_nativeAudioPlayer = nativeAudioPlayer;
		_encoder = encoder;
		_recorder = recorder;
		_streamer = streamer;
		_echo = echo;
		_photoCapture = photoCapture;
		_storageWatcher = storageWatcher;
		_videoMixer = videoMixer;
		_audioMixer = audioMixer;
		_outputConfigurer = outputConfigurer;
		_previewer = previewer;
		_eventBus = eventBus;
		_telemetryClient = telemetryClient;
		_videoCapturer = videoCapturer;
		_eventBridge = new LckPublicApiEventBridge(_eventBus);
		_eventBridge.Forward<LckEvents.RecordingStartedEvent, LckResult>(delegate(LckResult r)
		{
			LckMonoBehaviourMediator.Instance.EnqueueMainThreadAction(delegate
			{
				this.OnRecordingStarted?.Invoke(r);
			});
		});
		_eventBridge.Forward<LckEvents.RecordingPausedEvent, LckResult>(delegate(LckResult r)
		{
			this.OnRecordingPaused?.Invoke(r);
		});
		_eventBridge.Forward<LckEvents.RecordingResumedEvent, LckResult>(delegate(LckResult r)
		{
			this.OnRecordingResumed?.Invoke(r);
		});
		_eventBridge.Forward<LckEvents.RecordingStoppedEvent, LckResult>(delegate(LckResult r)
		{
			this.OnRecordingStopped?.Invoke(r);
		});
		_eventBridge.Forward<LckEvents.StreamingStartedEvent, LckResult>(delegate(LckResult r)
		{
			LckMonoBehaviourMediator.Instance.EnqueueMainThreadAction(delegate
			{
				this.OnStreamingStarted?.Invoke(r);
			});
		});
		_eventBridge.Forward<LckEvents.StreamingStoppedEvent, LckResult>(delegate(LckResult r)
		{
			this.OnStreamingStopped?.Invoke(r);
		});
		_eventBridge.Forward<LckEvents.PhotoCaptureSavedEvent, LckResult>(delegate(LckResult r)
		{
			this.OnPhotoSaved?.Invoke(r);
		});
		_eventBridge.Forward<LckEvents.LowStorageSpaceDetectedEvent, LckResult>(delegate(LckResult r)
		{
			this.OnLowStorageSpace?.Invoke(r);
		});
		_eventBridge.Forward((LckEvents.RecordingSavedEvent evt) => evt.SaveResult, delegate(LckResult<RecordingData> r)
		{
			this.OnRecordingSaved?.Invoke(r);
		});
		_eventBridge.Forward((LckEvents.EchoSavedEvent evt) => evt.SaveResult, delegate(LckResult<RecordingData> r)
		{
			this.OnEchoSaved?.Invoke(r);
		});
		_eventBridge.Forward<LckEvents.EchoEnabledEvent, LckResult>(delegate(LckResult r)
		{
			this.OnEchoEnabled?.Invoke(r);
		});
		_eventBus.AddListener(delegate(LckEvents.EchoDisabledEvent e)
		{
			this.OnEchoDisabled?.Invoke(e.Result, e.Reason);
		});
		_eventBridge.Forward((LckEvents.ActiveCameraChangedEvent evt) => evt.CameraResult, delegate(LckResult<ILckCamera> r)
		{
			this.OnActiveCameraSet?.Invoke(r);
		});
		_eventErrorLogger = new LckEventErrorLogger(_eventBus, delegate(ILckResult result)
		{
			LckLog.LogError($"{result.Error}: {result.Message}", ".ctor", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LckService.cs", 148);
		});
		_eventErrorLogger.Monitor<LckEvents.RecordingStartedEvent, LckResult>();
		_eventErrorLogger.Monitor<LckEvents.RecordingPausedEvent, LckResult>();
		_eventErrorLogger.Monitor<LckEvents.RecordingResumedEvent, LckResult>();
		_eventErrorLogger.Monitor<LckEvents.RecordingStoppedEvent, LckResult>();
		_eventErrorLogger.Monitor<LckEvents.StreamingStartedEvent, LckResult>();
		_eventErrorLogger.Monitor<LckEvents.StreamingStoppedEvent, LckResult>();
		_eventErrorLogger.Monitor<LckEvents.PhotoCaptureSavedEvent, LckResult>();
		_eventErrorLogger.Monitor<LckEvents.LowStorageSpaceDetectedEvent, LckResult>();
		Liv.NGFX.LogLevel nativeLogLevel = LckSettings.Instance.NativeLogLevel;
		NI.SetGlobalLogLevel(nativeLogLevel, LckSettings.Instance.ShowOpenGLMessages);
		_encoder.SetLogLevel(nativeLogLevel);
		_recorder.SetLogLevel(nativeLogLevel);
		_streamer?.SetLogLevel(nativeLogLevel);
		_videoCapturer.StartCapturing();
		if (VerifyGraphicsApi() && !Application.isEditor)
		{
			LckLog.Log(string.Format("LCK version is v{0}#{1}", "1.4.6", -1), ".ctor", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LckService.cs", 167);
		}
	}

	public static LckResult<LckService> GetService()
	{
		LckService lckService = (LckService)LckDiContainer.Instance.GetService<ILckService>();
		if (lckService == null)
		{
			return LckResult<LckService>.NewError(LckError.ServiceNotCreated, "Service not created");
		}
		return LckResult<LckService>.NewSuccess(lckService);
	}

	public LckResult StartRecording()
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return _recorder.StartRecording();
	}

	public LckResult PauseRecording()
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return _recorder.PauseRecording();
	}

	public LckResult ResumeRecording()
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return _recorder.ResumeRecording();
	}

	public LckResult StopRecording()
	{
		return StopRecording(StopReason.UserStopped);
	}

	public LckResult StartStreaming()
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		if (_streamer == null || _streamer is NullLckStreamer)
		{
			return LckResult.NewError(LckError.StreamerNotImplemented, "LCK streaming package not available");
		}
		return _streamer.StartStreaming();
	}

	public LckResult StopStreaming()
	{
		return StopStreaming(StopReason.UserStopped);
	}

	public LckResult StopStreaming(StopReason stopReason)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		if (_streamer == null || _streamer is NullLckStreamer)
		{
			return LckResult.NewError(LckError.StreamerNotImplemented, "LCK streaming package not available");
		}
		return _streamer.StopStreaming(stopReason);
	}

	public LckResult<TimeSpan> GetRecordingDuration()
	{
		return _recorder.GetRecordingDuration();
	}

	public LckResult<TimeSpan> GetStreamDuration()
	{
		return _streamer.GetStreamDuration();
	}

	public LckResult SetTrackResolution(CameraResolutionDescriptor cameraResolutionDescriptor)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		if (IsCapturing().Result)
		{
			return LckResult.NewError(LckError.CantEditSettingsWhileCapturing, "Can't change resolution while capturing.");
		}
		return _outputConfigurer.SetActiveResolution(cameraResolutionDescriptor);
	}

	public LckResult SetCameraOrientation(LckCameraOrientation orientation)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		if (IsCapturing().Result)
		{
			return LckResult.NewError(LckError.CantEditSettingsWhileCapturing, "Can't change camera orientation while capturing.");
		}
		return _outputConfigurer.SetCameraOrientation(orientation);
	}

	public LckResult SetTrackFramerate(uint framerate)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		if (IsCapturing().Result)
		{
			return LckResult.NewError(LckError.CantEditSettingsWhileCapturing, "Can't change framerate while capturing.");
		}
		return _outputConfigurer.SetActiveVideoFramerate(framerate);
	}

	public LckResult SetPreviewActive(bool isActive)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		_previewer.IsPreviewActive = isActive;
		return LckResult.NewSuccess();
	}

	public LckResult SetTrackDescriptor(CameraTrackDescriptor cameraTrackDescriptor)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		if (IsCapturing().Result)
		{
			return LckResult.NewError(LckError.CantEditSettingsWhileCapturing, "Can't change track descriptor while capturing.");
		}
		return _outputConfigurer.SetActiveCameraTrackDescriptor(cameraTrackDescriptor);
	}

	public LckResult SetTrackDescriptor(LckCaptureType captureType, CameraTrackDescriptor cameraTrackDescriptor)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		if (IsCapturing().Result)
		{
			return LckResult.NewError(LckError.CantEditSettingsWhileCapturing, "Can't change track descriptor while capturing.");
		}
		return _outputConfigurer.SetCameraTrackDescriptor(captureType, cameraTrackDescriptor);
	}

	public LckResult SetTrackBitrate(uint bitrate)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		if (IsCapturing().Result)
		{
			return LckResult.NewError(LckError.CantEditSettingsWhileCapturing, "Can't change video bitrate while capturing.");
		}
		return _outputConfigurer.SetActiveVideoBitrate(bitrate);
	}

	public LckResult SetTrackAudioBitrate(uint audioBitrate)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		if (IsCapturing().Result)
		{
			return LckResult.NewError(LckError.CantEditSettingsWhileCapturing, "Can't change audio bitrate while capturing.");
		}
		return _outputConfigurer.SetActiveAudioBitrate(audioBitrate);
	}

	public LckResult<bool> IsRecording()
	{
		if (_disposed)
		{
			return LckResult<bool>.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return _recorder.IsRecording();
	}

	public LckResult<bool> IsStreaming()
	{
		if (_disposed)
		{
			return LckResult<bool>.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		if (_streamer == null || _streamer is NullLckStreamer)
		{
			return LckResult<bool>.NewError(LckError.StreamerNotImplemented, "LCK streaming package not available");
		}
		return LckResult<bool>.NewSuccess(_streamer.IsStreaming);
	}

	public LckResult<bool> IsPaused()
	{
		if (_disposed)
		{
			return LckResult<bool>.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return _recorder.IsPaused();
	}

	public LckResult<bool> IsCapturing()
	{
		if (_disposed)
		{
			return LckResult<bool>.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return LckResult<bool>.NewSuccess(_encoder.IsActive());
	}

	public LckResult SetGameAudioCaptureActive(bool isActive)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return _audioMixer.SetGameAudioMute(!isActive);
	}

	public LckResult SetMicrophoneCaptureActive(bool isActive)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return _audioMixer.SetMicrophoneCaptureActive(isActive);
	}

	public LckResult<float> GetMicrophoneOutputLevel()
	{
		if (_disposed)
		{
			return LckResult<float>.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return LckResult<float>.NewSuccess(_audioMixer.GetMicrophoneOutputLevel());
	}

	public LckResult SetMicrophoneGain(float gain)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		_audioMixer.SetMicrophoneGain(gain);
		return LckResult.NewSuccess();
	}

	public LckResult SetGameAudioGain(float gain)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		_audioMixer.SetGameAudioGain(gain);
		return LckResult.NewSuccess();
	}

	public LckResult<float> GetGameOutputLevel()
	{
		if (_disposed)
		{
			return LckResult<float>.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return LckResult<float>.NewSuccess(_audioMixer.GetGameOutputLevel());
	}

	public LckResult<bool> IsGameAudioMute()
	{
		if (_disposed)
		{
			return LckResult<bool>.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return _audioMixer.IsGameAudioMute();
	}

	public LckResult SetActiveCamera(string cameraId, string monitorId = null)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return _videoMixer.ActivateCameraById(cameraId, monitorId);
	}

	public LckResult<ILckCamera> GetActiveCamera()
	{
		if (_disposed)
		{
			return LckResult<ILckCamera>.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return _videoMixer.GetActiveCamera();
	}

	public LckResult PreloadDiscreetAudio(AudioClip audioClip, float volume, bool forceReload = false)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		_nativeAudioPlayer?.PreloadAudioClip(audioClip, volume, forceReload);
		return LckResult.NewSuccess();
	}

	public LckResult PlayDiscreetAudioClip(AudioClip audioClip)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		_nativeAudioPlayer?.PlayAudioClip(audioClip, 1f);
		return LckResult.NewSuccess();
	}

	public LckResult StopAllDiscreetAudio()
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		_nativeAudioPlayer?.StopAllAudio();
		return LckResult.NewSuccess();
	}

	public LckResult SetEchoEnabled(bool enabled)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		_echo.SetEnabledAsync(enabled).ContinueWith(delegate(Task<LckResult> t)
		{
			if (t.IsFaulted)
			{
				LckLog.LogError($"SetEchoEnabled exception: {t.Exception}", "SetEchoEnabled", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LckService.cs", 592);
			}
			else if (!t.Result.Success)
			{
				LckLog.LogError("SetEchoEnabled failed: " + t.Result.Message, "SetEchoEnabled", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LckService.cs", 594);
			}
		});
		return LckResult.NewSuccess();
	}

	public async Task<LckResult> SetEchoEnabledAsync(bool enabled)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return await _echo.SetEnabledAsync(enabled);
	}

	public LckResult<bool> IsEchoEnabled()
	{
		if (_disposed)
		{
			return LckResult<bool>.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return LckResult<bool>.NewSuccess(_echo.IsEnabled);
	}

	public LckResult TriggerEchoSave()
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return _echo.TriggerSave();
	}

	public LckResult<TimeSpan> GetEchoBufferDuration()
	{
		if (_disposed)
		{
			return LckResult<TimeSpan>.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return LckResult<TimeSpan>.NewSuccess(_echo.GetBufferDuration());
	}

	public LckResult<TimeSpan> GetEchoMaxBufferDuration()
	{
		if (_disposed)
		{
			return LckResult<TimeSpan>.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return LckResult<TimeSpan>.NewSuccess(_echo.GetMaxBufferDuration());
	}

	public LckResult<LckDescriptor> GetDescriptor()
	{
		if (_disposed)
		{
			return LckResult<LckDescriptor>.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		LckResult<LckCaptureType> activeCaptureType = GetActiveCaptureType();
		if (!activeCaptureType.Success)
		{
			return LckResult<LckDescriptor>.NewError(LckError.UnknownError, "Failed to get active capture type");
		}
		LckCaptureType result = activeCaptureType.Result;
		LckResult<CameraTrackDescriptor> cameraTrackDescriptor = _outputConfigurer.GetCameraTrackDescriptor(result);
		if (!cameraTrackDescriptor.Success)
		{
			return LckResult<LckDescriptor>.NewError(LckError.UnknownError, "Failed to get camera track descriptor");
		}
		return LckResult<LckDescriptor>.NewSuccess(new LckDescriptor
		{
			cameraTrackDescriptor = cameraTrackDescriptor.Result
		});
	}

	public LckResult CapturePhoto()
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		if (_photoCapture == null)
		{
			return LckResult.NewError(LckError.PhotoCaptureError, "Failed to Capture Photo, LckPhotoCapture is null");
		}
		return _photoCapture.Capture();
	}

	public LckResult<LckCaptureType> GetActiveCaptureType()
	{
		if (_disposed)
		{
			return LckResult<LckCaptureType>.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return _outputConfigurer.GetActiveCaptureType();
	}

	public LckResult SetActiveCaptureType(LckCaptureType captureType)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return _outputConfigurer.SetActiveCaptureType(captureType);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_encodeLooper?.Dispose();
				_encodeLooper = null;
				_videoCapturer?.Dispose();
				_videoCapturer = null;
				_encoder?.Dispose();
				_encoder = null;
				_recorder?.Dispose();
				_recorder = null;
				_echo?.Dispose();
				_echo = null;
				_streamer?.Dispose();
				_streamer = null;
				_audioMixer?.Dispose();
				_audioMixer = null;
				_videoMixer?.Dispose();
				_videoMixer = null;
				_storageWatcher?.Dispose();
				_storageWatcher = null;
				_nativeAudioPlayer?.Dispose();
				_nativeAudioPlayer = null;
				_previewer?.Dispose();
				_previewer = null;
				_photoCapture?.Dispose();
				_photoCapture = null;
				_eventBridge?.Dispose();
				LckMonoBehaviourMediator.StopAllActiveCoroutines();
			}
			_telemetryClient.SendTelemetry(new LckTelemetryEvent(LckTelemetryEventType.ServiceDisposed));
			LckLog.Log("LCK service disposed.", "Dispose", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LckService.cs", 759);
			_disposed = true;
		}
	}

	~LckService()
	{
		Dispose(disposing: false);
	}

	internal LckResult StopRecording(StopReason stopReason = StopReason.UserStopped)
	{
		if (_disposed)
		{
			return LckResult.NewError(LckError.ServiceDisposed, "Service has been disposed");
		}
		return _recorder.StopRecording(stopReason);
	}

	internal static bool VerifyGraphicsApi()
	{
		GraphicsDeviceType graphicsDeviceType = SystemInfo.graphicsDeviceType;
		switch (Application.platform)
		{
		case RuntimePlatform.Android:
			if (graphicsDeviceType == GraphicsDeviceType.Vulkan || graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
			{
				return true;
			}
			LckLog.LogError("LCK requires Vulkan or OpenGLES3 graphics API on Android. Any other api is not supported.", "VerifyGraphicsApi", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LckService.cs", 790);
			return false;
		case RuntimePlatform.WindowsPlayer:
		case RuntimePlatform.WindowsEditor:
			if (graphicsDeviceType == GraphicsDeviceType.Vulkan || graphicsDeviceType == GraphicsDeviceType.Direct3D11 || graphicsDeviceType == GraphicsDeviceType.OpenGLCore)
			{
				return true;
			}
			LckLog.LogError("LCK requires the Vulkan, OpenGLCore or DirectX 11 graphics API on Windows. Any other api is not supported.", "VerifyGraphicsApi", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LckService.cs", 799);
			return false;
		default:
			return false;
		}
	}

	internal static bool VerifyPlatform()
	{
		int num;
		if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.WindowsPlayer)
		{
			num = ((Application.platform == RuntimePlatform.WindowsEditor) ? 1 : 0);
			if (num == 0)
			{
				LckLog.LogError($"LCK is not supported on {Application.platform}.", "VerifyPlatform", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LckService.cs", 810);
			}
		}
		else
		{
			num = 1;
		}
		return (byte)num != 0;
	}
}
