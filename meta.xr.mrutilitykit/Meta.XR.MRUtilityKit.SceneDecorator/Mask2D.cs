using System;
using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public abstract class Mask2D : Mask
{
	[SerializeField]
	public float offsetX;

	[SerializeField]
	public float offsetY;

	[SerializeField]
	public float rotation;

	[SerializeField]
	public float scaleX = 1f;

	[SerializeField]
	public float scaleY = 1f;

	[SerializeField]
	public float shearX;

	[SerializeField]
	public float shearY;

	private static Float3X3 GenerateAffineTransform(Vector2 position, float rotation, Vector2 scale, Vector2 shear)
	{
		float f = MathF.PI / 180f * rotation;
		float num = Mathf.Cos(f);
		Float3X3 b2 = Float3X3.Multiply(b: new Float3X3(scale.x, 0f, 0f, 0f, scale.y, 0f, 0f, 0f, 1f), a: new Float3X3(1f, shear.x, 0f, shear.y, 1f, 0f, 0f, 0f, 1f));
		f = Mathf.Sin(f);
		b2 = Float3X3.Multiply(new Float3X3(num, 0f - f, 0f, f, num, 0f, 0f, 0f, 1f), b2);
		return Float3X3.Multiply(new Float3X3(1f, 0f, position.x, 0f, 1f, position.y, 0f, 0f, 1f), b2);
	}

	internal static Float3X3 GenerateAffineTransform(float positionX, float positionY, float rotation, float scaleX, float scaleY, float shearX, float shearY)
	{
		return GenerateAffineTransform(new Vector2(positionX, positionY), rotation, new Vector2(scaleX, scaleY), new Vector2(shearX, shearY));
	}
}
