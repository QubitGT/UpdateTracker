using System;
using UnityEngine;

namespace Oculus.Interaction;

public class ConeUtils
{
	public static bool RayWithinCone(Ray ray, Vector3 position, float apertureDegrees)
	{
		float num = Mathf.Cos(apertureDegrees * (MathF.PI / 180f));
		Vector3 lhs = position - ray.origin;
		float magnitude = lhs.magnitude;
		if (Mathf.Abs(magnitude) < 0.001f)
		{
			return true;
		}
		lhs /= magnitude;
		return Vector3.Dot(lhs, ray.direction) >= num;
	}
}
