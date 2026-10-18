using System;
using Unity.Mathematics;

namespace UnityEngine.Rendering;

[Serializable]
internal struct AABB
{
	public float3 center;

	public float3 extents;

	public float3 min => center - extents;

	public float3 max => center + extents;

	public override string ToString()
	{
		return $"AABB(Center:{center}, Extents:{extents}";
	}

	private static float3 RotateExtents(float3 extents, float3 m0, float3 m1, float3 m2)
	{
		return math.abs(m0 * extents.x) + math.abs(m1 * extents.y) + math.abs(m2 * extents.z);
	}

	public static AABB Transform(float4x4 transform, AABB localBounds)
	{
		AABB result = default(AABB);
		result.extents = RotateExtents(localBounds.extents, transform.c0.xyz, transform.c1.xyz, transform.c2.xyz);
		result.center = math.transform(transform, localBounds.center);
		return result;
	}
}
