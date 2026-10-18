using System;

namespace Liv.Lck;

[Serializable]
public struct CameraTrackDescriptor
{
	public CameraResolutionDescriptor CameraResolutionDescriptor;

	public uint Bitrate;

	public uint Framerate;

	public uint AudioBitrate;

	public CameraTrackDescriptor(CameraResolutionDescriptor cameraResolutionDescriptor, uint bitrate = 5242880u, uint framerate = 30u, uint audioBitrate = 192000u)
	{
		CameraResolutionDescriptor = cameraResolutionDescriptor;
		Bitrate = bitrate;
		Framerate = framerate;
		AudioBitrate = audioBitrate;
	}
}
