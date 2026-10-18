namespace Photon.Voice;

internal class AudioInEnumeratorNotSupported : DeviceEnumeratorNotSupported
{
	public AudioInEnumeratorNotSupported(ILogger logger)
		: base(logger, "Current platform is not supported by audio input DeviceEnumerator.")
	{
	}
}
