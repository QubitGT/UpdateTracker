namespace Liv.Lck;

public enum LckError
{
	ServiceNotCreated = 1,
	ServiceDisposed,
	InvalidDescriptor,
	CameraIdNotFound,
	MonitorIdNotFound,
	MicrophonePermissionDenied,
	CaptureAlreadyStarted,
	NotCurrentlyRecording,
	NotPaused,
	RecordingError,
	PhotoCaptureError,
	CantEditSettingsWhileCapturing,
	NotEnoughStorageSpace,
	FailedToCopyRecordingToGallery,
	FailedToCopyPhotoToGallery,
	UnsupportedGraphicsApi,
	UnsupportedPlatform,
	MicrophoneError,
	StreamerNotImplemented,
	StreamingError,
	EncodingError,
	EchoError,
	UnknownError
}
