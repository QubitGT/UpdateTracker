using UnityEngine;

namespace Fusion.LagCompensation;

public readonly struct AABB
{
	public readonly Vector3 Center;

	public readonly Vector3 Extents;

	public readonly Vector3 Min;

	public readonly Vector3 Max;

	public AABB(Bounds bounds)
	{
		Center = bounds.center;
		Extents = bounds.extents;
		Min = bounds.min;
		Max = bounds.max;
	}

	public AABB(Vector3 center, Vector3 extents)
	{
		Center = center;
		Extents = extents;
		Min = center - extents;
		Max = center + extents;
	}

	public AABB(Vector3 center, Vector3 pointA, Vector3 pointB)
	{
		Max = default(Vector3);
		Center = center;
		Extents = Max - Center;
		Min = Vector3.Min(pointA, pointB);
		Max = Vector3.Max(pointA, pointB);
	}
}
