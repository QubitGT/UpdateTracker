using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Unity.Cinemachine;

internal static class SplineContainerExtensions
{
	public static bool IsValid(this ISplineContainer spline)
	{
		if (spline != null && spline.Splines != null)
		{
			return spline.Splines.Count > 0;
		}
		return false;
	}

	public static bool LocalEvaluateSplineWithRoll(this ISpline spline, float tNormalized, CinemachineSplineRoll roll, out Vector3 position, out Quaternion rotation)
	{
		if (spline == null || !spline.Evaluate(tNormalized, out var position2, out var tangent, out var upVector))
		{
			position = Vector3.zero;
			rotation = Quaternion.identity;
			return false;
		}
		Vector3 v = Vector3.Cross(tangent, upVector);
		if (v.AlmostZero() || v.IsNaN())
		{
			tangent = Vector3.forward;
			upVector = Vector3.up;
		}
		if (roll == null || !roll.enabled)
		{
			rotation = Quaternion.LookRotation(tangent, upVector);
		}
		else
		{
			float angle2 = roll.Roll.Evaluate(spline, tNormalized, PathIndexUnit.Normalized, roll.GetInterpolator());
			rotation = Quaternion.LookRotation(tangent, upVector) * RollAroundForward(angle2);
		}
		position = position2;
		return true;
		static Quaternion RollAroundForward(float angle)
		{
			float f = angle * 0.5f * (MathF.PI / 180f);
			return new Quaternion(0f, 0f, Mathf.Sin(f), Mathf.Cos(f));
		}
	}

	public static bool EvaluateSplineWithRoll(this ISpline spline, Transform transform, float tNormalized, CinemachineSplineRoll roll, out Vector3 position, out Quaternion rotation)
	{
		bool result = spline.LocalEvaluateSplineWithRoll(tNormalized, roll, out position, out rotation);
		position = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).MultiplyPoint3x4(position);
		rotation = transform.rotation * rotation;
		return result;
	}

	public static Vector3 EvaluateSplinePosition(this ISpline spline, Transform transform, float tNormalized)
	{
		float3 @float = spline?.EvaluatePosition(tNormalized) ?? default(float3);
		if (!(transform == null))
		{
			return Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).MultiplyPoint3x4(@float);
		}
		return @float;
	}

	public static float GetMaxPosition(this ISpline spline, PathIndexUnit unit)
	{
		switch (unit)
		{
		case PathIndexUnit.Distance:
			return spline.GetLength();
		case PathIndexUnit.Knot:
		{
			int count = spline.Count;
			return (!spline.Closed || count < 2) ? Mathf.Max(0, count - 1) : count;
		}
		default:
			return 1f;
		}
	}

	public static float StandardizePosition(this ISpline spline, float t, PathIndexUnit unit, out float maxPos)
	{
		maxPos = spline.GetMaxPosition(unit);
		if (float.IsNaN(t))
		{
			return 0f;
		}
		if (!spline.Closed)
		{
			return Mathf.Clamp(t, 0f, maxPos);
		}
		t %= maxPos;
		if (t < 0f)
		{
			t += maxPos;
		}
		return t;
	}
}
