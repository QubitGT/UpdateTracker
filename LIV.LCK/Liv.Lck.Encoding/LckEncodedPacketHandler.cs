namespace Liv.Lck.Encoding;

internal struct LckEncodedPacketHandler
{
	public ILckCaptureStateProvider CaptureStateProvider { get; }

	public LckEncodedPacketCallback EncodedPacketCallback { get; }

	public LckEncodedPacketHandler(ILckCaptureStateProvider captureStateProvider, LckEncodedPacketCallback encodedPacketCallback)
	{
		CaptureStateProvider = captureStateProvider;
		EncodedPacketCallback = encodedPacketCallback;
	}
}
