using Meta.XR.MRUtilityKit.Extensions;
using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class SimplexNoiseMask : Mask2D
{
	public override float SampleMask(Candidate c)
	{
		Vector3 vector = Float3X3.Multiply(Mask2D.GenerateAffineTransform(offsetX, offsetY, rotation, scaleX, scaleY, shearX, shearY), Vector3Extensions.FromVector2AndZ(c.localPos, 1f));
		vector /= vector.z;
		return Mathf.Abs(SimplexNoise.srdnoise(new Vector2(vector.x, vector.y), 0f).x);
	}

	public override bool Check(Candidate c)
	{
		return true;
	}
}
