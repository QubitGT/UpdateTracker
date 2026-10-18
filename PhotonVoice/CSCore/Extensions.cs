using System;

namespace CSCore;

public static class Extensions
{
	internal static bool IsPCM(this WaveFormat waveFormat)
	{
		if (waveFormat == null)
		{
			throw new ArgumentNullException("waveFormat");
		}
		if (waveFormat is WaveFormatExtensible)
		{
			return ((WaveFormatExtensible)waveFormat).SubFormat == AudioSubTypes.Pcm;
		}
		return waveFormat.WaveFormatTag == AudioEncoding.Pcm;
	}

	internal static bool IsIeeeFloat(this WaveFormat waveFormat)
	{
		if (waveFormat == null)
		{
			throw new ArgumentNullException("waveFormat");
		}
		if (waveFormat is WaveFormatExtensible)
		{
			return ((WaveFormatExtensible)waveFormat).SubFormat == AudioSubTypes.IeeeFloat;
		}
		return waveFormat.WaveFormatTag == AudioEncoding.IeeeFloat;
	}
}
