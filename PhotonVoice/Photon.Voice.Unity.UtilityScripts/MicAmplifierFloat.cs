using System;

namespace Photon.Voice.Unity.UtilityScripts;

public class MicAmplifierFloat : IProcessor<float>, IDisposable
{
	public float AmplificationFactor { get; set; }

	public float BoostValue { get; set; }

	public float MaxBefore { get; private set; }

	public float MaxAfter { get; private set; }

	public bool Disabled { get; set; }

	public MicAmplifierFloat(float amplificationFactor, float boostValue)
	{
		AmplificationFactor = amplificationFactor;
		BoostValue = boostValue;
	}

	public float[] Process(float[] buf)
	{
		if (Disabled)
		{
			return buf;
		}
		for (int i = 0; i < buf.Length; i++)
		{
			float num = buf[i];
			buf[i] *= AmplificationFactor;
			buf[i] += BoostValue;
			if (MaxBefore < num)
			{
				MaxBefore = num;
				MaxAfter = buf[i];
			}
		}
		return buf;
	}

	public void Dispose()
	{
	}
}
