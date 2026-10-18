using Meta.XR.MRUtilityKit.Extensions;
using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class CookieMask : Mask2D
{
	public enum SampleMode
	{
		NEAREST,
		NEAREST_REPEAT,
		NEAREST_REPEAT_MIRROR,
		BILINEAR,
		BILINEAR_REPEAT,
		BILINEAR_REPEAT_MIRROR
	}

	[SerializeField]
	public Texture2D cookie;

	[SerializeField]
	public SampleMode sampleMode;

	public override float SampleMask(Candidate c)
	{
		Vector3 vector = Float3X3.Multiply(Mask2D.GenerateAffineTransform(offsetX, offsetY, rotation, scaleX, scaleY, shearX, shearY), Vector3Extensions.FromVector2AndZ(c.localPos, 1f));
		vector /= vector.z;
		Vector2 vector2 = new Vector2(vector.x, vector.y);
		switch (sampleMode)
		{
		default:
			vector2 *= new Vector2(cookie.width, cookie.height);
			return ((vector.x < 0f) | (vector.x > 1f) | (vector.y < 0f) | (vector.y > 1f)) ? 0f : cookie.GetPixel((int)vector2.x, (int)vector2.y).r;
		case SampleMode.NEAREST_REPEAT:
			vector2 = vector2.Frac();
			vector2 *= new Vector2(cookie.width, cookie.height);
			return cookie.GetPixel((int)vector2.x, (int)vector2.y).r;
		case SampleMode.NEAREST_REPEAT_MIRROR:
			vector2 = 2f * (vector2 - vector2.Add(0.5f).Floor()).Abs();
			vector2 *= new Vector2(cookie.width, cookie.height);
			return cookie.GetPixel((int)vector2.x, (int)vector2.y).r;
		case SampleMode.BILINEAR:
			return ((vector2.x < 0f) | (vector2.x > 1f) | (vector2.y < 0f) | (vector2.y > 1f)) ? 0f : cookie.GetPixelBilinear(vector2.x, vector2.y).r;
		case SampleMode.BILINEAR_REPEAT:
			vector2 = vector2.Frac();
			return cookie.GetPixelBilinear(vector2.x, vector2.y).r;
		case SampleMode.BILINEAR_REPEAT_MIRROR:
			vector2 = 2f * (vector2 - vector2.Add(0.5f).Floor()).Abs();
			return cookie.GetPixelBilinear(vector2.x, vector2.y).r;
		}
	}

	public override bool Check(Candidate c)
	{
		return true;
	}
}
