using Liv.Lck.ErrorHandling;
using Liv.Lck.Recorder;
using UnityEngine;

namespace Liv.Lck;

internal class LckEvents
{
	internal interface IEventWithResult<out TResult> where TResult : ILckResult
	{
		TResult Result { get; }
	}

	internal struct EncoderStartedEvent : IEventWithResult<LckResult>
	{
		public LckResult Result { get; }

		public EncoderStartedEvent(LckResult result)
		{
			Result = result;
		}
	}

	internal struct EncoderStoppedEvent : IEventWithResult<LckResult>
	{
		public LckResult Result { get; }

		public EncoderStoppedEvent(LckResult result)
		{
			Result = result;
		}
	}

	internal struct RecordingStartedEvent : IEventWithResult<LckResult>
	{
		public LckResult Result { get; }

		public RecordingStartedEvent(LckResult result)
		{
			Result = result;
		}
	}

	internal struct RecordingPausedEvent : IEventWithResult<LckResult>
	{
		public LckResult Result { get; }

		public RecordingPausedEvent(LckResult result)
		{
			Result = result;
		}
	}

	internal struct RecordingResumedEvent : IEventWithResult<LckResult>
	{
		public LckResult Result { get; }

		public RecordingResumedEvent(LckResult result)
		{
			Result = result;
		}
	}

	internal struct RecordingStoppedEvent : IEventWithResult<LckResult>
	{
		public LckResult Result { get; }

		public RecordingStoppedEvent(LckResult result)
		{
			Result = result;
		}
	}

	internal struct RecordingSavedEvent : IEventWithResult<LckResult<RecordingData>>
	{
		public LckResult<RecordingData> SaveResult { get; }

		public LckResult<RecordingData> Result => SaveResult;

		public RecordingSavedEvent(LckResult<RecordingData> saveResult)
		{
			SaveResult = saveResult;
		}
	}

	internal struct EchoSavedEvent : IEventWithResult<LckResult<RecordingData>>
	{
		public LckResult<RecordingData> SaveResult { get; }

		public LckResult<RecordingData> Result => SaveResult;

		public EchoSavedEvent(LckResult<RecordingData> saveResult)
		{
			SaveResult = saveResult;
		}
	}

	internal struct EchoEnabledEvent : IEventWithResult<LckResult>
	{
		public LckResult Result { get; }

		public EchoEnabledEvent(LckResult result)
		{
			Result = result;
		}
	}

	internal struct EchoDisabledEvent : IEventWithResult<LckResult>
	{
		public LckResult Result { get; }

		public EchoDisableReason Reason { get; }

		public EchoDisabledEvent(LckResult result, EchoDisableReason reason = EchoDisableReason.User)
		{
			Result = result;
			Reason = reason;
		}
	}

	internal struct StreamingStartedEvent : IEventWithResult<LckResult>
	{
		public LckResult Result { get; }

		public StreamingStartedEvent(LckResult result)
		{
			Result = result;
		}
	}

	internal struct StreamingStoppedEvent : IEventWithResult<LckResult>
	{
		public LckResult Result { get; }

		public StreamingStoppedEvent(LckResult result)
		{
			Result = result;
		}
	}

	internal struct PhotoCaptureSavedEvent : IEventWithResult<LckResult>
	{
		public LckResult Result { get; }

		public PhotoCaptureSavedEvent(LckResult result)
		{
			Result = result;
		}
	}

	internal struct LowStorageSpaceDetectedEvent : IEventWithResult<LckResult>
	{
		public LckResult Result { get; }

		public LowStorageSpaceDetectedEvent(LckResult result)
		{
			Result = result;
		}
	}

	internal struct ActiveCameraChangedEvent : IEventWithResult<LckResult<ILckCamera>>
	{
		public LckResult<ILckCamera> CameraResult { get; }

		public LckResult<ILckCamera> Result => CameraResult;

		public ActiveCameraChangedEvent(LckResult<ILckCamera> cameraResult)
		{
			CameraResult = cameraResult;
		}
	}

	internal struct ActiveCameraTrackTextureChangedEvent : IEventWithResult<LckResult<RenderTexture>>
	{
		public LckResult<RenderTexture> CameraTrackTextureResult { get; }

		public LckResult<RenderTexture> Result => CameraTrackTextureResult;

		public ActiveCameraTrackTextureChangedEvent(LckResult<RenderTexture> cameraTrackTextureResult)
		{
			CameraTrackTextureResult = cameraTrackTextureResult;
		}
	}

	internal struct CameraResolutionChangedEvent : IEventWithResult<LckResult<CameraResolutionDescriptor>>
	{
		public LckResult<CameraResolutionDescriptor> Result { get; }

		public CameraResolutionChangedEvent(LckResult<CameraResolutionDescriptor> cameraResult)
		{
			Result = cameraResult;
		}
	}

	internal struct CameraFramerateChangedEvent : IEventWithResult<LckResult<uint>>
	{
		public LckResult<uint> Result { get; }

		public CameraFramerateChangedEvent(LckResult<uint> result)
		{
			Result = result;
		}
	}

	internal struct CaptureErrorEvent
	{
		public LckCaptureError Error { get; }

		public CaptureErrorEvent(LckCaptureError error)
		{
			Error = error;
		}
	}
}
