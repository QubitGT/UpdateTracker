using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class RandomMask : Mask
{
	public override float SampleMask(Candidate c)
	{
		return Random.value;
	}

	public override bool Check(Candidate c)
	{
		return true;
	}
}
