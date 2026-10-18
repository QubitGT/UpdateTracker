using System;
using Photon.Voice.Windows;

namespace Photon.Voice;

public static class Platform
{
	public static IDeviceEnumerator CreateAudioInEnumerator(ILogger logger)
	{
		return new AudioInEnumerator(logger);
	}

	public static IAudioInChangeNotifier CreateAudioInChangeNotifier(Action callback, ILogger logger)
	{
		return new AudioInChangeNotifierNotSupported(callback, logger);
	}

	public static IEncoder CreateDefaultAudioEncoder<T>(ILogger logger, VoiceInfo info)
	{
		return info.Codec switch
		{
			Codec.AudioOpus => OpusCodec.Factory.CreateEncoder<T[]>(info, logger), 
			Codec.Raw => new RawCodec.Encoder<T>(), 
			_ => throw new UnsupportedCodecException("Platform.CreateDefaultAudioEncoder", info.Codec), 
		};
	}

	public static IAudioDesc CreateDefaultAudioSource(ILogger logger, DeviceInfo dev, int samplingRate, int channels, object otherParams = null)
	{
		return new WindowsAudioInPusher(dev.IsDefault ? (-1) : dev.IDInt, logger);
	}
}
