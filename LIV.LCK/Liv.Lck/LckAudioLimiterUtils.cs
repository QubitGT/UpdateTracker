using System;
using UnityEngine;

namespace Liv.Lck;

internal static class LckAudioLimiterUtils
{
	public static float ApplySoftClip(float audioIn)
	{
		return audioIn / (0.75f + Mathf.Abs(audioIn * 0.75f));
	}

	public static float CalculateAttackCoefficient(float attackTime, int sampleRate)
	{
		return (float)Math.Exp(-1.0 / (double)(attackTime * (float)sampleRate));
	}

	public static float CalculateReleaseCoefficient(float releaseTime, int sampleRate)
	{
		return (float)Math.Exp(-1.0 / (double)(releaseTime * (float)sampleRate));
	}

	public static float UpdateEnvelope(float gainReduction, float envelope, float attackCoeff, float releaseCoeff)
	{
		if (gainReduction < envelope)
		{
			return attackCoeff * envelope + (1f - attackCoeff) * gainReduction;
		}
		return releaseCoeff * envelope + (1f - releaseCoeff) * gainReduction;
	}

	public static float ApplyGainReduction(float data, float envelope, float makeUpGain)
	{
		return data * envelope * makeUpGain;
	}
}
