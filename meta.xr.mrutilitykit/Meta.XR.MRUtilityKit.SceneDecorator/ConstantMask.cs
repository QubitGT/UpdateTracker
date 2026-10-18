using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class ConstantMask : Mask
{
	[SerializeField]
	public float constant;

	public override float SampleMask(Candidate c)
	{
		return constant;
	}

	public override bool Check(Candidate c)
	{
		return true;
	}
}
