using Meta.XR.Util;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class SlopeMask : Mask
{
	public override float SampleMask(Candidate c)
	{
		return c.slope;
	}

	public override bool Check(Candidate c)
	{
		return true;
	}
}
