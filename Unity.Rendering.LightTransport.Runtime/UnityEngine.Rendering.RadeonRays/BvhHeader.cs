using Unity.Mathematics;

namespace UnityEngine.Rendering.RadeonRays;

internal struct BvhHeader
{
	public uint internalNodeCount;

	public uint leafNodeCount;

	public uint root;

	public uint unused;

	public float3 globalAabbMin;

	public float3 globalAabbMax;

	public uint3 unused3;

	public uint3 unused4;
}
