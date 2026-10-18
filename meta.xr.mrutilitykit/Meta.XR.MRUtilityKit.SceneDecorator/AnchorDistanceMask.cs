using Meta.XR.Util;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class AnchorDistanceMask : Mask
{
	public override float SampleMask(Candidate c)
	{
		return c.anchorDist;
	}

	public override bool Check(Candidate c)
	{
		return true;
	}
}
