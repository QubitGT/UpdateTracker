using System;
using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class CompositeMaskAdd : Mask2D
{
	[Serializable]
	public struct MaskLayer
	{
		public Mask mask;

		public float outputScale;

		public float outputLimitMin;

		public float outputLimitMax;

		public float outputOffset;

		public float SampleMask(Candidate c)
		{
			return mask.SampleMask(c, outputLimitMin, outputLimitMax, outputScale, outputOffset);
		}
	}

	[SerializeField]
	private MaskLayer[] maskLayers;

	public override float SampleMask(Candidate c)
	{
		Vector3 vector = Float3X3.Multiply(Mask2D.GenerateAffineTransform(offsetX, offsetY, rotation, scaleX, scaleY, shearX, shearY), new Vector3(c.localPos.x, c.localPos.y, 1f));
		vector /= vector.z;
		c.localPos = new Vector2(vector.x, vector.y);
		float num = 0f;
		MaskLayer[] array = maskLayers;
		foreach (MaskLayer maskLayer in array)
		{
			num += maskLayer.SampleMask(c);
		}
		return num;
	}

	public override bool Check(Candidate c)
	{
		return true;
	}
}
