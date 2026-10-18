using UnityEngine;

namespace Liv.NativeAudioBridge;

public static class NativeAudioUtils
{
	public static sbyte[] ConvertAudioClipToByteArray(AudioClip audioClip, float volume = 1f)
	{
		float[] array = new float[audioClip.samples * audioClip.channels];
		audioClip.GetData(array, 0);
		sbyte[] array2 = new sbyte[array.Length * 2];
		int num = 32767;
		for (int i = 0; i < array.Length; i++)
		{
			short num2 = (short)(Mathf.Clamp(array[i] * volume, -1f, 1f) * (float)num);
			array2[i * 2] = (sbyte)(num2 & 0xFF);
			array2[i * 2 + 1] = (sbyte)((num2 & 0xFF00) >> 8);
		}
		return array2;
	}
}
