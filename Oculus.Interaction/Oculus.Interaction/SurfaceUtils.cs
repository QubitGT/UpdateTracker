using Oculus.Interaction.Surfaces;
using UnityEngine;

namespace Oculus.Interaction;

public static class SurfaceUtils
{
	public static float ComputeDistanceAbove(ISurfacePatch surfacePatch, Vector3 point, float radius)
	{
		surfacePatch.BackingSurface.ClosestSurfacePoint(in point, out var hit);
		return Vector3.Dot(point - hit.Point, hit.Normal) - radius;
	}

	public static float ComputeTangentDistance(ISurfacePatch surfacePatch, Vector3 point, float radius)
	{
		surfacePatch.ClosestSurfacePoint(in point, out var hit);
		surfacePatch.BackingSurface.ClosestSurfacePoint(in point, out var hit2);
		Vector3 vector = point - hit.Point;
		Vector3 vector2 = Vector3.Dot(vector, hit2.Normal) * hit2.Normal;
		return (vector - vector2).magnitude - radius;
	}

	public static float ComputeDepth(ISurfacePatch surfacePatch, Vector3 point, float radius)
	{
		return Mathf.Max(0f, 0f - ComputeDistanceAbove(surfacePatch, point, radius));
	}

	public static float ComputeDistanceFrom(ISurfacePatch surfacePatch, Vector3 point, float radius)
	{
		surfacePatch.ClosestSurfacePoint(in point, out var hit);
		return (point - hit.Point).magnitude - radius;
	}
}
