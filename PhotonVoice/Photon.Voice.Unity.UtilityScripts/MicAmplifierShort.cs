using System;

namespace Photon.Voice.Unity.UtilityScripts;

public class MicAmplifierShort : IProcessor<short>, IDisposable
{
	public short AmplificationFactor { get; set; }

	public short BoostValue { get; set; }

	public short MaxBefore { get; private set; }

	public short MaxAfter { get; private set; }

	public bool Disabled { get; set; }

	public MicAmplifierShort(short amplificationFactor, short boostValue)
	{
		AmplificationFactor = amplificationFactor;
		BoostValue = boostValue;
	}

	public short[] Process(short[] buf)
	{
		if (Disabled)
		{
			return buf;
		}
		for (int i = 0; i < buf.Length; i++)
		{
			short num = buf[i];
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
