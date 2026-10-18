namespace Liv.Lck.Core;

public enum LckTelemetryEventType : uint
{
	GameInitialized,
	RecordingStarted,
	StreamingStarted,
	ServiceCreated,
	ServiceDisposed,
	CameraEnabled,
	CameraDisabled,
	RecordingStopped,
	StreamingStopped,
	StreamingError,
	PhotoCaptured,
	RecorderError,
	PhotoCaptureError,
	SdkError,
	Performance,
	EchoEnabled,
	EchoSaved
}
