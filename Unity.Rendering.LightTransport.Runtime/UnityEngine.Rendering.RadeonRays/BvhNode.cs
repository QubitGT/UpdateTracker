using Unity.Mathematics;

namespace UnityEngine.Rendering.RadeonRays;

internal struct BvhNode
{
	public uint child0;

	public uint child1;

	public uint parent;

	public uint update;

	public float3 aabb0_min;

	public float3 aabb0_max;

	public float3 aabb1_min;

	public float3 aabb1_max;
}
