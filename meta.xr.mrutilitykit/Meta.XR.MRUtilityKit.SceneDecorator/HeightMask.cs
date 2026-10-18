using Meta.XR.Util;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class HeightMask : Mask
{
	public override float SampleMask(Candidate c)
	{
		return c.hit.point.y;
	}

	public override bool Check(Candidate c)
	{
		return true;
	}
}
