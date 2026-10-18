namespace Photon.Voice;

internal class VideoInEnumeratorNotSupported : DeviceEnumeratorNotSupported
{
	public VideoInEnumeratorNotSupported(ILogger logger)
		: base(logger, "Current platform is not supported by video capture DeviceEnumerator.")
	{
	}
}
