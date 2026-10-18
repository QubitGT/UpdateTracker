using UnityEngine;

namespace Fusion.LagCompensation;

public class BVHNodeDrawInfo
{
	internal HitboxBuffer Buffer;

	internal int NodeIndex;

	public Bounds Bounds => Buffer.BVH.GetNode(NodeIndex).Box;

	public int Depth => Buffer.BVH.GetNode(NodeIndex).Depth;

	public int MaxDepth => Buffer.BVH.maxDepth;

	internal BVHNodeDrawInfo(HitboxBuffer buffer)
	{
		Buffer = buffer;
	}

	internal BVHNodeDrawInfo FromBVHNode(ref BVHNode node)
	{
		NodeIndex = node.Index;
		return this;
	}
}
