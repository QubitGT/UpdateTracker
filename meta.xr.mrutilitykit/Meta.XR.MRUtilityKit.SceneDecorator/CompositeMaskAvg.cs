using Meta.XR.MRUtilityKit.Extensions;
using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class CompositeMaskAvg : Mask2D
{
	[SerializeField]
	private CompositeMaskAdd.MaskLayer[] maskLayers;

	public override float SampleMask(Candidate c)
	{
		Vector3 vector = Float3X3.Multiply(Mask2D.GenerateAffineTransform(offsetX, offsetY, rotation, scaleX, scaleY, shearX, shearY), Vector3Extensions.FromVector2AndZ(c.localPos, 1f));
		vector /= vector.z;
		c.localPos = new Vector2(vector.x, vector.y);
		float num = 0f;
		CompositeMaskAdd.MaskLayer[] array = maskLayers;
		foreach (CompositeMaskAdd.MaskLayer maskLayer in array)
		{
			num += maskLayer.SampleMask(c);
		}
		return num / (float)maskLayers.Length;
	}

	public override bool Check(Candidate c)
	{
		return true;
	}
}
