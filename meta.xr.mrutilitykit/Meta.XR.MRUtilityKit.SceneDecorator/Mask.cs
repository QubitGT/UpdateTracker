using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public abstract class Mask : ScriptableObject
{
	public abstract float SampleMask(Candidate candidate);

	public abstract bool Check(Candidate c);

	public float SampleMask(Candidate candidate, float scale, float offset = 0f)
	{
		return scale * SampleMask(candidate) + offset;
	}

	public float SampleMask(Candidate candidate, float limitMin, float limitMax, float scale = 1f, float offset = 0f)
	{
		float num = scale * SampleMask(candidate) + offset;
		if (!(limitMin > limitMax))
		{
			return Mathf.Clamp(num, limitMin, limitMax);
		}
		return Mathf.Clamp(limitMin - num, limitMax, limitMin);
	}
}
