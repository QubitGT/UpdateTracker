using UnityEngine;
using UnityEngine.Scripting;

namespace Liv.Lck;

internal class LckOutputConfigurer : ILckOutputConfigurer
{
	private readonly ILckEventBus _eventBus;

	private CameraTrackDescriptor _recordingCameraTrackDescriptor;

	private CameraTrackDescriptor _streamingCameraTrackDescriptor;

	private LckCaptureType _activeCaptureType;

	[Preserve]
	public LckOutputConfigurer(ILckQualityConfig qualityConfig, ILckEventBus eventBus)
	{
		_eventBus = eventBus;
		ConfigureDefaultSettings(qualityConfig);
	}

	public LckResult ConfigureFromQualityConfig(QualityOption qualityOption)
	{
		bool flag = IsValidDescriptor(qualityOption.RecordingCameraTrackDescriptor);
		if (flag)
		{
			SetCameraTrackDescriptor(LckCaptureType.Recording, qualityOption.RecordingCameraTrackDescriptor);
		}
		bool flag2 = IsValidDescriptor(qualityOption.StreamingCameraTrackDescriptor);
		if (flag2)
		{
			SetCameraTrackDescriptor(LckCaptureType.Streaming, qualityOption.StreamingCameraTrackDescriptor);
		}
		return CreateQualityConfigurationResult(qualityOption, flag, flag2);
	}

	public LckResult<LckCaptureType> GetActiveCaptureType()
	{
		return LckResult<LckCaptureType>.NewSuccess(_activeCaptureType);
	}

	public LckResult SetActiveCaptureType(LckCaptureType captureType)
	{
		if (_activeCaptureType != captureType)
		{
			_activeCaptureType = captureType;
			OnActiveCameraTrackDescriptorChanged();
		}
		return LckResult.NewSuccess();
	}

	public LckResult SetActiveVideoFramerate(uint framerate)
	{
		switch (_activeCaptureType)
		{
		case LckCaptureType.Recording:
			_recordingCameraTrackDescriptor.Framerate = framerate;
			break;
		case LckCaptureType.Streaming:
			_streamingCameraTrackDescriptor.Framerate = framerate;
			break;
		default:
			return NewUnknownCaptureTypeError();
		}
		TriggerCameraFramerateChangedEvent(framerate);
		return LckResult.NewSuccess();
	}

	public LckResult SetActiveVideoBitrate(uint bitrate)
	{
		switch (_activeCaptureType)
		{
		case LckCaptureType.Recording:
			_recordingCameraTrackDescriptor.Bitrate = bitrate;
			break;
		case LckCaptureType.Streaming:
			_streamingCameraTrackDescriptor.Bitrate = bitrate;
			break;
		default:
			return NewUnknownCaptureTypeError();
		}
		return LckResult.NewSuccess();
	}

	public LckResult SetActiveAudioBitrate(uint bitrate)
	{
		switch (_activeCaptureType)
		{
		case LckCaptureType.Recording:
			_recordingCameraTrackDescriptor.AudioBitrate = bitrate;
			break;
		case LckCaptureType.Streaming:
			_streamingCameraTrackDescriptor.AudioBitrate = bitrate;
			break;
		default:
			return NewUnknownCaptureTypeError();
		}
		return LckResult.NewSuccess();
	}

	public LckResult SetActiveResolution(CameraResolutionDescriptor resolution)
	{
		return SetResolution(_activeCaptureType, resolution);
	}

	public LckResult<CameraTrackDescriptor> GetCameraTrackDescriptor(LckCaptureType captureType)
	{
		return captureType switch
		{
			LckCaptureType.Recording => LckResult<CameraTrackDescriptor>.NewSuccess(_recordingCameraTrackDescriptor), 
			LckCaptureType.Streaming => LckResult<CameraTrackDescriptor>.NewSuccess(_streamingCameraTrackDescriptor), 
			_ => NewUnknownCaptureTypeError<CameraTrackDescriptor>(), 
		};
	}

	public LckResult SetCameraTrackDescriptor(LckCaptureType captureType, CameraTrackDescriptor trackDescriptor)
	{
		switch (captureType)
		{
		case LckCaptureType.Recording:
			_recordingCameraTrackDescriptor = trackDescriptor;
			break;
		case LckCaptureType.Streaming:
			_streamingCameraTrackDescriptor = trackDescriptor;
			break;
		default:
			return LckResult.NewError(LckError.UnknownError, "Unknown capture type");
		}
		if (captureType == _activeCaptureType)
		{
			OnActiveCameraTrackDescriptorChanged();
		}
		return LckResult.NewSuccess();
	}

	public LckResult SetCameraOrientation(LckCameraOrientation orientation)
	{
		LckResult lckResult = SetCameraOrientation(LckCaptureType.Recording, orientation);
		LckResult lckResult2 = SetCameraOrientation(LckCaptureType.Streaming, orientation);
		if (lckResult.Success && lckResult2.Success)
		{
			return LckResult.NewSuccess();
		}
		string text = "SetCameraOrientation failed with the following errors: ";
		if (!lckResult.Success)
		{
			text = text + "\n  - " + lckResult.Message;
		}
		if (!lckResult2.Success)
		{
			text = text + "\n  - " + lckResult2.Message;
		}
		return LckResult.NewError(LckError.UnknownError, text);
	}

	public LckResult<CameraTrackDescriptor> GetActiveCameraTrackDescriptor()
	{
		return _activeCaptureType switch
		{
			LckCaptureType.Recording => LckResult<CameraTrackDescriptor>.NewSuccess(_recordingCameraTrackDescriptor), 
			LckCaptureType.Streaming => LckResult<CameraTrackDescriptor>.NewSuccess(_streamingCameraTrackDescriptor), 
			_ => NewUnknownCaptureTypeError<CameraTrackDescriptor>(), 
		};
	}

	public LckResult SetActiveCameraTrackDescriptor(CameraTrackDescriptor trackDescriptor)
	{
		return SetCameraTrackDescriptor(_activeCaptureType, trackDescriptor);
	}

	public LckResult<uint> GetNumberOfAudioChannels()
	{
		return LckResult<uint>.NewSuccess(2u);
	}

	public LckResult<uint> GetAudioSampleRate()
	{
		int num = DetermineAudioSystemSampleRate();
		if (num <= 0)
		{
			return LckResult<uint>.NewError(LckError.UnknownError, $"Invalid audio sample rate retrieved from audio system: {num}Hz");
		}
		return LckResult<uint>.NewSuccess((uint)num);
	}

	private void ConfigureDefaultSettings(ILckQualityConfig qualityConfig)
	{
		QualityOption qualityOption = qualityConfig.GetQualityOptionsForSystem().Find((QualityOption option) => option.IsDefault);
		LckResult lckResult = ConfigureFromQualityConfig(qualityOption);
		if (!lckResult.Success)
		{
			LckLog.LogError("LCK: Failed to configure default output settings - " + lckResult.Message, "ConfigureDefaultSettings", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LckOutputConfigurer.cs", 208);
		}
	}

	private void TriggerCameraResolutionChangedEvent(CameraResolutionDescriptor resolution)
	{
		LckResult<CameraResolutionDescriptor> cameraResult = LckResult<CameraResolutionDescriptor>.NewSuccess(resolution);
		_eventBus.Trigger(new LckEvents.CameraResolutionChangedEvent(cameraResult));
	}

	private void TriggerCameraFramerateChangedEvent(uint framerate)
	{
		LckResult<uint> result = LckResult<uint>.NewSuccess(framerate);
		_eventBus.Trigger(new LckEvents.CameraFramerateChangedEvent(result));
	}

	private void OnActiveCameraTrackDescriptorChanged()
	{
		CameraTrackDescriptor result = GetActiveCameraTrackDescriptor().Result;
		TriggerCameraFramerateChangedEvent(result.Framerate);
		TriggerCameraResolutionChangedEvent(result.CameraResolutionDescriptor);
	}

	private CameraResolutionDescriptor GetResolution(LckCaptureType captureType)
	{
		return GetCameraTrackDescriptor(captureType).Result.CameraResolutionDescriptor;
	}

	private LckResult SetResolution(LckCaptureType captureType, CameraResolutionDescriptor resolution)
	{
		switch (captureType)
		{
		case LckCaptureType.Recording:
			_recordingCameraTrackDescriptor.CameraResolutionDescriptor = resolution;
			break;
		case LckCaptureType.Streaming:
			_streamingCameraTrackDescriptor.CameraResolutionDescriptor = resolution;
			break;
		default:
			return NewUnknownCaptureTypeError();
		}
		if (captureType == _activeCaptureType)
		{
			TriggerCameraResolutionChangedEvent(resolution);
		}
		return LckResult.NewSuccess();
	}

	private LckResult SetCameraOrientation(LckCaptureType captureType, LckCameraOrientation orientation)
	{
		CameraResolutionDescriptor resolution = GetResolution(captureType);
		if (GetCameraOrientation(resolution) == orientation)
		{
			return LckResult.NewSuccess();
		}
		CameraResolutionDescriptor resolutionInOrientation = resolution.GetResolutionInOrientation(orientation);
		return SetResolution(captureType, resolutionInOrientation);
	}

	private static LckResult CreateQualityConfigurationResult(QualityOption qualityOption, bool isRecordingValid, bool isStreamingValid)
	{
		if (isRecordingValid && isStreamingValid)
		{
			return LckResult.NewSuccess();
		}
		string text = "QualityOption (" + qualityOption.Name + ") has an invalid CameraTrackDescriptor for the following capture type(s): ";
		if (!isRecordingValid)
		{
			text += "\n  - Recording";
		}
		if (!isStreamingValid)
		{
			text += "\n  - Streaming";
		}
		return LckResult.NewError(LckError.InvalidDescriptor, text);
	}

	private static int DetermineAudioSystemSampleRate()
	{
		return AudioSettings.outputSampleRate;
	}

	private static bool IsValidDescriptor(CameraTrackDescriptor descriptor)
	{
		if (descriptor.CameraResolutionDescriptor.IsValid() && descriptor.Bitrate != 0 && descriptor.Framerate != 0)
		{
			return descriptor.AudioBitrate != 0;
		}
		return false;
	}

	private static LckCameraOrientation GetCameraOrientation(CameraResolutionDescriptor resolution)
	{
		if (resolution.Width < resolution.Height)
		{
			return LckCameraOrientation.Portrait;
		}
		return LckCameraOrientation.Landscape;
	}

	private static LckResult<T> NewUnknownCaptureTypeError<T>()
	{
		return LckResult<T>.NewError(LckError.UnknownError, "Unknown capture type");
	}

	private static LckResult NewUnknownCaptureTypeError()
	{
		return LckResult.NewError(LckError.UnknownError, "Unknown capture type");
	}
}
