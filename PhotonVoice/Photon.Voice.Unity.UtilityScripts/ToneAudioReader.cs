using System;
using UnityEngine;

namespace Photon.Voice.Unity.UtilityScripts;

internal class ToneAudioReader : IAudioReader<float>, IDataReader<float>, IDisposable, IAudioDesc
{
	private double k;

	private long timeSamples;

	public int Channels => 2;

	public int SamplingRate => 24000;

	public string Error => null;

	public ToneAudioReader()
	{
		k = Math.PI * 880.0 / (double)SamplingRate;
	}

	public void Dispose()
	{
	}

	public bool Read(float[] buf)
	{
		int num = buf.Length / Channels;
		long num2 = (long)(AudioSettings.dspTime * (double)SamplingRate);
		long num3 = num2 - timeSamples;
		if (Math.Abs(num3) > SamplingRate / 4)
		{
			Debug.LogWarningFormat("ToneAudioReader sample time is out: {0} / {1}", timeSamples, num2);
			num3 = num;
			timeSamples = num2 - num;
		}
		if (num3 < num)
		{
			return false;
		}
		int num4 = 0;
		for (int i = 0; i < num; i++)
		{
			float num5 = (float)Math.Sin((double)timeSamples++ * k) * 0.2f;
			for (int j = 0; j < Channels; j++)
			{
				buf[num4++] = num5;
			}
		}
		return true;
	}
}
