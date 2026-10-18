using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

[Serializable]
public struct OneEuroFilterPropertyBlock
{
	[SerializeField]
	[Tooltip("Decrease min cutoff until jitter is eliminated")]
	public float _minCutoff;

	[SerializeField]
	[Tooltip("Increase beta from zero to reduce lag")]
	public float _beta;

	[SerializeField]
	[Tooltip("Smaller values of dCutoff smooth more but slow accuracy")]
	public float _dCutoff;

	public float MinCutoff => _minCutoff;

	public float Beta => _beta;

	public float DCutoff => _dCutoff;

	private static float DefaultMinCutoff => 1f;

	private static float DefaultBeta => 0f;

	private static float DefaultDCutoff => 1f;

	public static OneEuroFilterPropertyBlock Default
	{
		get
		{
			OneEuroFilterPropertyBlock result = default(OneEuroFilterPropertyBlock);
			result._minCutoff = DefaultMinCutoff;
			result._beta = DefaultBeta;
			result._dCutoff = DefaultDCutoff;
			return result;
		}
	}

	public OneEuroFilterPropertyBlock(float minCutoff, float beta, float dCutoff)
	{
		_minCutoff = minCutoff;
		_beta = beta;
		_dCutoff = dCutoff;
	}

	public OneEuroFilterPropertyBlock(float minCutoff, float beta)
	{
		_minCutoff = minCutoff;
		_beta = beta;
		_dCutoff = DefaultDCutoff;
	}
}
