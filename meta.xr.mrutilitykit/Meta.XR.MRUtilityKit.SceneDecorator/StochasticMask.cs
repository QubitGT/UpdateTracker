using Meta.XR.MRUtilityKit.Extensions;
using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class StochasticMask : Mask2D
{
	[SerializeField]
	public CompositeMaskAdd.MaskLayer probabilitySource;

	public override float SampleMask(Candidate c)
	{
		Vector3 vector = Float3X3.Multiply(Mask2D.GenerateAffineTransform(offsetX, offsetY, rotation, scaleX, scaleY, shearX, shearY), Vector3Extensions.FromVector2AndZ(c.localPos, 1f));
		vector /= vector.z;
		c.localPos = new Vector2(vector.x, vector.y);
		if (!(Random.value < probabilitySource.SampleMask(c)))
		{
			return 0f;
		}
		return 1f;
	}

	public override bool Check(Candidate c)
	{
		return true;
	}
}
