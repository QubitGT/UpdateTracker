using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class AnchorComponentDistanceMask : Mask
{
	public enum Axis
	{
		X,
		Y,
		Z
	}

	[SerializeField]
	public Axis axis;

	public override float SampleMask(Candidate c)
	{
		return c.anchorCompDists[(int)axis];
	}

	public override bool Check(Candidate c)
	{
		return true;
	}
}
