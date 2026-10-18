using Liv.Lck.Collections;

namespace Liv.Lck.Utilities;

public static class ChannelMixingUtils
{
	public const int MonoChannelCount = 1;

	public const int StereoChannelCount = 2;

	public const int FiveOneChannelCount = 6;

	public static void ConvertMonoToStereo(float[] sourceMonoAudio, int sourceAudioStartIdx, int sourceAudioLength, AudioBuffer outputBuffer)
	{
		outputBuffer.Clear();
		for (int i = 0; i < sourceAudioLength; i++)
		{
			float value = sourceMonoAudio[sourceAudioStartIdx + i];
			for (int j = 0; j < 2; j++)
			{
				outputBuffer.TryAdd(value);
			}
		}
	}

	public static void ConvertFiveOneToStereo(float[] sourceFiveOneAudio, int sourceAudioStartIdx, int sourceAudioLength, AudioBuffer outputBuffer)
	{
		int num = sourceAudioLength / 6;
		outputBuffer.Clear();
		for (int i = 0; i < num; i++)
		{
			int num2 = sourceAudioStartIdx + i * 6;
			float num3 = sourceFiveOneAudio[num2];
			float num4 = sourceFiveOneAudio[num2 + 1];
			float num5 = sourceFiveOneAudio[num2 + 2];
			float num6 = sourceFiveOneAudio[num2 + 4];
			float num7 = sourceFiveOneAudio[num2 + 5];
			float value = 0.707f * num3 + 0.5f * num5 + 0.354f * num6;
			float value2 = 0.707f * num4 + 0.5f * num5 + 0.354f * num7;
			outputBuffer.TryAdd(value);
			outputBuffer.TryAdd(value2);
		}
	}
}
