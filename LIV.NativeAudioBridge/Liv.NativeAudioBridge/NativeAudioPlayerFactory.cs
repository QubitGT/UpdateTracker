namespace Liv.NativeAudioBridge;

public static class NativeAudioPlayerFactory
{
	public static INativeAudioPlayer CreateNativeAudioPlayer()
	{
		return new NativeAudioPlayerWindows();
	}
}
