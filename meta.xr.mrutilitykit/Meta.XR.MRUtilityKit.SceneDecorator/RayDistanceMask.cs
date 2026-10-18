using Meta.XR.Util;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class RayDistanceMask : Mask
{
	public override float SampleMask(Candidate c)
	{
		return c.hit.distance;
	}

	public override bool Check(Candidate c)
	{
		return true;
	}
}
