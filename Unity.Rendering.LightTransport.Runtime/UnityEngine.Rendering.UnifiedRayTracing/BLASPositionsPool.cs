using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.Rendering.RadeonRays;

namespace UnityEngine.Rendering.UnifiedRayTracing;

internal sealed class BLASPositionsPool : IDisposable
{
	public const int VertexSizeInDwords = 3;

	private const int intialVertexCount = 1000;

	private GraphicsBuffer m_VerticesBuffer;

	private BlockAllocator m_VerticesAllocator;

	private readonly ComputeShader m_CopyPositionsShader;

	private readonly int m_CopyVerticesKernel;

	private readonly ComputeShader m_CopyShader;

	private const uint kItemsPerWorkgroup = 6144u;

	public GraphicsBuffer VertexBuffer => m_VerticesBuffer;

	public BLASPositionsPool(ComputeShader copyPositionsShader, ComputeShader copyShader)
	{
		m_VerticesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 3000, 4);
		m_VerticesAllocator = default(BlockAllocator);
		m_VerticesAllocator.Initialize(1000);
		m_CopyPositionsShader = copyPositionsShader;
		m_CopyVerticesKernel = m_CopyPositionsShader.FindKernel("CopyVertexBuffer");
		m_CopyShader = copyShader;
	}

	public void Dispose()
	{
		m_VerticesBuffer.Dispose();
		m_VerticesAllocator.Dispose();
	}

	public void Clear()
	{
		m_VerticesBuffer.Dispose();
		m_VerticesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 3000, 4);
		m_VerticesAllocator.Dispose();
		m_VerticesAllocator = default(BlockAllocator);
		m_VerticesAllocator.Initialize(1000);
	}

	public bool Add(VertexBufferChunk info, out BlockAllocator.Allocation verticesAllocation)
	{
		verticesAllocation = m_VerticesAllocator.Allocate((int)(info.vertexCount * 3));
		if (!verticesAllocation.valid)
		{
			verticesAllocation = m_VerticesAllocator.GrowAndAllocate((int)info.vertexCount, int.MaxValue / UnsafeUtility.SizeOf<float3>(), out var oldCapacity, out var newCapacity);
			if (!verticesAllocation.valid)
			{
				return false;
			}
			GraphicsHelpers.ReallocateBuffer(m_CopyShader, oldCapacity, newCapacity, UnsafeUtility.SizeOf<float3>(), ref m_VerticesBuffer);
		}
		CommandBuffer commandBuffer = new CommandBuffer();
		commandBuffer.SetComputeIntParam(m_CopyPositionsShader, "_InputPosBufferCount", (int)info.vertexCount);
		commandBuffer.SetComputeIntParam(m_CopyPositionsShader, "_InputPosBufferOffset", info.verticesStartOffset);
		commandBuffer.SetComputeIntParam(m_CopyPositionsShader, "_InputBaseVertex", info.baseVertex);
		commandBuffer.SetComputeIntParam(m_CopyPositionsShader, "_InputPosBufferStride", (int)info.vertexStride);
		commandBuffer.SetComputeIntParam(m_CopyPositionsShader, "_OutputPosBufferOffset", verticesAllocation.block.offset * 3);
		commandBuffer.SetComputeBufferParam(m_CopyPositionsShader, m_CopyVerticesKernel, "_InputPosBuffer", info.vertices);
		commandBuffer.SetComputeBufferParam(m_CopyPositionsShader, m_CopyVerticesKernel, "_OutputPosBuffer", m_VerticesBuffer);
		commandBuffer.DispatchCompute(m_CopyPositionsShader, m_CopyVerticesKernel, (int)Common.CeilDivide(info.vertexCount, 6144u), 1, 1);
		Graphics.ExecuteCommandBuffer(commandBuffer);
		return true;
	}

	public void Remove(ref BlockAllocator.Allocation verticesAllocation)
	{
		m_VerticesAllocator.FreeAllocation(in verticesAllocation);
		verticesAllocation = BlockAllocator.Allocation.Invalid;
	}
}
