using System.Collections.Generic;
using Unity.Mathematics;

namespace UnityEngine.Rendering.RadeonRays;

internal class BvhCheck
{
	public class VertexBuffers
	{
		public GraphicsBuffer vertices;

		public GraphicsBuffer indices;

		public uint vertexBufferOffset;

		public uint vertexCount;

		public uint vertexStride = 3u;

		public uint indexBufferOffset;

		public IndexFormat indexFormat;

		public uint indexCount;
	}

	private sealed class VertexBuffersCPU
	{
		public float[] vertices;

		public uint[] indices;

		public uint vertexStride;
	}

	private struct Triangle
	{
		public float3 v0;

		public float3 v1;

		public float3 v2;
	}

	private const uint kInvalidID = uint.MaxValue;

	public static VertexBuffers Convert(MeshBuildInfo info)
	{
		return new VertexBuffers
		{
			vertices = info.vertices,
			indices = info.triangleIndices,
			vertexBufferOffset = (uint)info.verticesStartOffset,
			vertexCount = info.vertexCount,
			vertexStride = info.vertexStride,
			indexBufferOffset = (uint)info.indicesStartOffset,
			indexCount = info.triangleCount * 3,
			indexFormat = info.indexFormat
		};
	}

	public static double SurfaceArea(AABB aabb)
	{
		float3 @float = aabb.Max - aabb.Min;
		return 2f * (@float.x * @float.y + @float.x * @float.z + @float.z * @float.y);
	}

	public static double NodeSahCost(uint nodeAddr, AABB nodeAabb, AABB parentAabb)
	{
		double num = (IsLeafNode(nodeAddr) ? ((float)GetLeafNodePrimCount(nodeAddr)) : 1.2f);
		double num2 = SurfaceArea(nodeAabb);
		double num3 = SurfaceArea(parentAabb);
		return num * num2 / num3;
	}

	public static double CheckConsistency(VertexBuffers bvhVertexBuffers, BottomLevelLevelAccelStruct bvh, uint primitiveCount)
	{
		return CheckConsistency(bvhVertexBuffers, bvh.bvh, bvh.bvhOffset, bvh.bvhLeaves, bvh.bvhLeavesOffset, primitiveCount);
	}

	public static double CheckConsistency(GraphicsBuffer bvhBuffer, uint bvhBufferOffset, uint primitiveCount)
	{
		return CheckConsistency(null, bvhBuffer, bvhBufferOffset, null, 0u, primitiveCount);
	}

	private static double CheckConsistency(VertexBuffers bvhVertexBuffers, GraphicsBuffer bvhBuffer, uint bvhBufferOffset, GraphicsBuffer bvhLeavesBuffer, uint bvhLeavesBufferOffset, uint primitiveCount)
	{
		BvhHeader[] array = new BvhHeader[1];
		bvhBuffer.GetData(array, 0, (int)bvhBufferOffset, 1);
		return CheckConsistency(bvhVertexBuffers, bvhBuffer, bvhBufferOffset + 1, bvhLeavesBuffer, bvhLeavesBufferOffset, array[0], primitiveCount);
	}

	public static int ExtractBits(uint value, int startBit, int count)
	{
		return (int)((uint)((1 << count) - 1 << startBit) & value) >> startBit;
	}

	public static bool IsLeafNode(uint nodeAddr)
	{
		return (nodeAddr & int.MinValue) != 0;
	}

	public static uint GetLeafNodeFirstPrim(uint nodeAddr)
	{
		return nodeAddr & 0x1FFFFFFFu;
	}

	public static uint GetLeafNodePrimCount(uint nodeAddr)
	{
		return (uint)(ExtractBits(nodeAddr, 29, 2) + 1);
	}

	private static double CheckConsistency(VertexBuffers bvhVertexBuffers, GraphicsBuffer bvhBuffer, uint bvhBufferOffset, GraphicsBuffer bvhLeavesBuffer, uint bvhLeavesBufferOffset, BvhHeader header, uint primitiveCount)
	{
		uint leafNodeCount = header.leafNodeCount;
		uint root = header.root;
		uint bvhNodeCount = HlbvhBuilder.GetBvhNodeCount(leafNodeCount);
		bool flag = bvhVertexBuffers == null;
		BvhNode[] array = new BvhNode[bvhNodeCount];
		bvhBuffer.GetData(array, 0, (int)bvhBufferOffset, (int)bvhNodeCount);
		VertexBuffersCPU bvhVertexBuffers2 = null;
		uint4[] array2 = null;
		if (!flag)
		{
			bvhVertexBuffers2 = DownloadVertexData(bvhVertexBuffers);
			array2 = new uint4[primitiveCount];
			bvhLeavesBuffer.GetData(array2, 0, (int)bvhLeavesBufferOffset, (int)primitiveCount);
		}
		uint num = 0u;
		AABB aabb = GetAabb(bvhVertexBuffers2, array, array2, root, flag);
		double num2 = 0.0;
		Queue<(uint, uint)> queue = new Queue<(uint, uint)>();
		queue.Enqueue((root, uint.MaxValue));
		while (queue.Count != 0)
		{
			uint item = queue.Dequeue().Item1;
			AABB aabb2 = GetAabb(bvhVertexBuffers2, array, array2, item, flag);
			num2 += NodeSahCost(item, aabb2, aabb);
			if (flag)
			{
				IsLeafNode(item);
			}
			if (IsLeafNode(item))
			{
				num += (flag ? 1 : GetLeafNodePrimCount(item));
				continue;
			}
			BvhNode bvhNode = array[item];
			AABB aabb3 = GetAabb(bvhVertexBuffers2, array, array2, bvhNode.child0, flag);
			AABB aabb4 = GetAabb(bvhVertexBuffers2, array, array2, bvhNode.child1, flag);
			aabb2.Contains(aabb3);
			aabb2.Contains(aabb4);
			queue.Enqueue((bvhNode.child0, item));
			queue.Enqueue((bvhNode.child1, item));
		}
		return num2;
	}

	private static uint3 GetFaceIndices(uint[] indices, uint triangleIdx)
	{
		return new uint3(indices[3 * triangleIdx], indices[3 * triangleIdx + 1], indices[3 * triangleIdx + 2]);
	}

	private static float3 GetVertex(float[] vertices, uint stride, uint idx)
	{
		uint num = idx * stride;
		return new float3(vertices[num], vertices[num + 1], vertices[num + 2]);
	}

	private static Triangle GetTriangle(float[] vertices, uint stride, uint3 idx)
	{
		Triangle result = default(Triangle);
		result.v0 = GetVertex(vertices, stride, idx.x);
		result.v1 = GetVertex(vertices, stride, idx.y);
		result.v2 = GetVertex(vertices, stride, idx.z);
		return result;
	}

	private static VertexBuffersCPU DownloadVertexData(VertexBuffers vertexBuffers)
	{
		VertexBuffersCPU vertexBuffersCPU = new VertexBuffersCPU();
		vertexBuffersCPU.vertices = new float[vertexBuffers.vertexCount * vertexBuffers.vertexStride];
		vertexBuffersCPU.indices = new uint[vertexBuffers.indexCount];
		vertexBuffersCPU.vertexStride = vertexBuffers.vertexStride;
		if (vertexBuffers.indexFormat == IndexFormat.Int32)
		{
			vertexBuffers.indices.GetData(vertexBuffersCPU.indices, 0, (int)vertexBuffers.indexBufferOffset, (int)vertexBuffers.indexCount);
		}
		else
		{
			ushort[] array = new ushort[vertexBuffers.indexCount];
			vertexBuffers.indices.GetData(array, 0, (int)vertexBuffers.indexBufferOffset, (int)vertexBuffers.indexCount);
			for (int i = 0; i < vertexBuffers.indexCount; i++)
			{
				vertexBuffersCPU.indices[i] = array[i];
			}
		}
		vertexBuffers.vertices.GetData(vertexBuffersCPU.vertices, 0, (int)vertexBuffers.vertexBufferOffset, (int)(vertexBuffers.vertexCount * vertexBuffers.vertexStride));
		return vertexBuffersCPU;
	}

	private static AABB GetAabb(VertexBuffersCPU bvhVertexBuffers, BvhNode[] bvhNodes, uint4[] bvhLeafNodes, uint nodeAddr, bool isTopLevel)
	{
		AABB aABB = new AABB();
		if (!IsLeafNode(nodeAddr))
		{
			BvhNode bvhNode = bvhNodes[nodeAddr];
			AABB aabb = new AABB(bvhNode.aabb0_min, bvhNode.aabb0_max);
			aABB.Encapsulate(aabb);
			AABB aabb2 = new AABB(bvhNode.aabb1_min, bvhNode.aabb1_max);
			aABB.Encapsulate(aabb2);
		}
		else if (!isTopLevel)
		{
			int leafNodeFirstPrim = (int)GetLeafNodeFirstPrim(nodeAddr);
			int leafNodePrimCount = (int)GetLeafNodePrimCount(nodeAddr);
			for (int i = 0; i < leafNodePrimCount; i++)
			{
				uint num = (uint)(i + leafNodeFirstPrim);
				uint3 xyz = bvhLeafNodes[num].xyz;
				GetFaceIndices(bvhVertexBuffers.indices, bvhLeafNodes[num].w);
				Triangle triangle = GetTriangle(bvhVertexBuffers.vertices, bvhVertexBuffers.vertexStride, xyz);
				aABB.Encapsulate(triangle.v0);
				aABB.Encapsulate(triangle.v1);
				aABB.Encapsulate(triangle.v2);
			}
		}
		return aABB;
	}
}
