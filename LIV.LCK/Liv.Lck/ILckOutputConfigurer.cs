namespace Liv.Lck;

internal interface ILckOutputConfigurer
{
	LckResult ConfigureFromQualityConfig(QualityOption qualityOption);

	LckResult<LckCaptureType> GetActiveCaptureType();

	LckResult SetActiveCaptureType(LckCaptureType captureType);

	LckResult SetActiveVideoFramerate(uint framerate);

	LckResult SetActiveVideoBitrate(uint bitrate);

	LckResult SetActiveAudioBitrate(uint bitrate);

	LckResult SetActiveResolution(CameraResolutionDescriptor resolution);

	LckResult SetCameraOrientation(LckCameraOrientation orientation);

	LckResult<CameraTrackDescriptor> GetCameraTrackDescriptor(LckCaptureType captureType);

	LckResult SetCameraTrackDescriptor(LckCaptureType captureType, CameraTrackDescriptor trackDescriptor);

	LckResult<CameraTrackDescriptor> GetActiveCameraTrackDescriptor();

	LckResult SetActiveCameraTrackDescriptor(CameraTrackDescriptor trackDescriptor);

	LckResult<uint> GetNumberOfAudioChannels();

	LckResult<uint> GetAudioSampleRate();
}
