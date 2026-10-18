using Unity.Mathematics;

namespace UnityEngine.Rendering.RadeonRays;

internal class HlbvhBuilder
{
	private struct ScratchBufferLayout
	{
		public uint PrimitiveRefs;

		public uint MortonCodes;

		public uint SortedPrimitiveRefs;

		public uint SortedMortonCodes;

		public uint SortMemory;

		public uint Aabb;

		public uint LeafParents;

		public uint InternalNodeRange;

		public uint TotalSize;

		public static ScratchBufferLayout Create(uint triangleCount)
		{
			ScratchBufferLayout result = default(ScratchBufferLayout);
			result.SortMemory = result.Reserve(math.max((uint)RadixSort.GetScratchDataSizeInDwords(triangleCount), triangleCount));
			result.PrimitiveRefs = result.Reserve(triangleCount);
			result.MortonCodes = result.Reserve(triangleCount);
			result.SortedPrimitiveRefs = result.Reserve(triangleCount);
			result.SortedMortonCodes = result.Reserve(triangleCount);
			result.Aabb = result.Reserve(6u);
			result.InternalNodeRange = result.PrimitiveRefs;
			result.LeafParents = result.SortMemory;
			return result;
		}

		private uint Reserve(uint size)
		{
			uint totalSize = TotalSize;
			TotalSize += size;
			return totalSize;
		}
	}

	private readonly ComputeShader shaderBuildHlbvh;

	private readonly int kernelInit;

	private readonly int kernelCalculateAabb;

	private readonly int kernelCalculateMortonCodes;

	private readonly int kernelBuildTreeBottomUp;

	private readonly RadixSort radixSort;

	private const uint kTrianglesPerThread = 8u;

	private const uint kGroupSize = 256u;

	private const uint kTrianglesPerGroup = 2048u;

	public HlbvhBuilder(RadeonRaysShaders shaders)
	{
		shaderBuildHlbvh = shaders.buildHlbvh;
		kernelInit = shaderBuildHlbvh.FindKernel("Init");
		kernelCalculateAabb = shaderBuildHlbvh.FindKernel("CalculateAabb");
		kernelCalculateMortonCodes = shaderBuildHlbvh.FindKernel("CalculateMortonCodes");
		kernelBuildTreeBottomUp = shaderBuildHlbvh.FindKernel("BuildTreeBottomUp");
		radixSort = new RadixSort(shaders);
	}

	public uint GetScratchDataSizeInDwords(uint triangleCount)
	{
		return ScratchBufferLayout.Create(triangleCount).TotalSize;
	}

	public static uint GetBvhNodeCount(uint leafCount)
	{
		return leafCount - 1;
	}

	public uint GetResultDataSizeInDwords(uint triangleCount)
	{
		uint num = GetBvhNodeCount(triangleCount) + 1;
		uint num2 = 16u;
		return num * num2;
	}

	public void Execute(CommandBuffer cmd, GraphicsBuffer vertices, int verticesOffset, uint vertexStride, GraphicsBuffer indices, int indicesOffset, int baseIndex, IndexFormat indexFormat, uint triangleCount, GraphicsBuffer scratch, in BottomLevelLevelAccelStruct result)
	{
		Common.EnableKeyword(cmd, shaderBuildHlbvh, "TOP_LEVEL", enable: false);
		Common.EnableKeyword(cmd, shaderBuildHlbvh, "UINT16_INDICES", indexFormat == IndexFormat.Int16);
		ScratchBufferLayout scratchLayout = ScratchBufferLayout.Create(triangleCount);
		cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_indices_offset, indicesOffset);
		cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_base_index, baseIndex);
		cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_vertices_offset, verticesOffset);
		cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_constants_vertex_stride, (int)vertexStride);
		cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_constants_triangle_count, (int)triangleCount);
		cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_bvh_offset, (int)result.bvhOffset);
		cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_bvh_leaves_offset, (int)result.bvhLeavesOffset);
		cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_internal_node_range_offset, (int)scratchLayout.InternalNodeRange);
		cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_leaf_parents_offset, (int)scratchLayout.LeafParents);
		cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_aabb_offset, (int)scratchLayout.Aabb);
		BindKernelArguments(cmd, kernelInit, vertices, indices, scratch, scratchLayout, result, setSortedCodes: false);
		cmd.DispatchCompute(shaderBuildHlbvh, kernelInit, 1, 1, 1);
		BindKernelArguments(cmd, kernelCalculateAabb, vertices, indices, scratch, scratchLayout, result, setSortedCodes: false);
		cmd.DispatchCompute(shaderBuildHlbvh, kernelCalculateAabb, (int)Common.CeilDivide(triangleCount, 2048u), 1, 1);
		BindKernelArguments(cmd, kernelCalculateMortonCodes, vertices, indices, scratch, scratchLayout, result, setSortedCodes: false);
		cmd.DispatchCompute(shaderBuildHlbvh, kernelCalculateMortonCodes, (int)Common.CeilDivide(triangleCount, 2048u), 1, 1);
		radixSort.Execute(cmd, scratch, scratchLayout.MortonCodes, scratchLayout.SortedMortonCodes, scratchLayout.PrimitiveRefs, scratchLayout.SortedPrimitiveRefs, scratchLayout.SortMemory, triangleCount);
		BindKernelArguments(cmd, kernelBuildTreeBottomUp, vertices, indices, scratch, scratchLayout, result, setSortedCodes: true);
		cmd.DispatchCompute(shaderBuildHlbvh, kernelBuildTreeBottomUp, (int)Common.CeilDivide(triangleCount, 2048u), 1, 1);
	}

	private void BindKernelArguments(CommandBuffer cmd, int kernel, GraphicsBuffer vertices, GraphicsBuffer indices, GraphicsBuffer scratch, ScratchBufferLayout scratchLayout, BottomLevelLevelAccelStruct result, bool setSortedCodes)
	{
		cmd.SetComputeBufferParam(shaderBuildHlbvh, kernel, SID.g_vertices, vertices);
		cmd.SetComputeBufferParam(shaderBuildHlbvh, kernel, SID.g_indices, indices);
		cmd.SetComputeBufferParam(shaderBuildHlbvh, kernel, SID.g_scratch_buffer, scratch);
		cmd.SetComputeBufferParam(shaderBuildHlbvh, kernel, SID.g_bvh, result.bvh);
		cmd.SetComputeBufferParam(shaderBuildHlbvh, kernel, SID.g_bvh_leaves, result.bvhLeaves);
		if (setSortedCodes)
		{
			cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_morton_codes_offset, (int)scratchLayout.SortedMortonCodes);
			cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_primitive_refs_offset, (int)scratchLayout.SortedPrimitiveRefs);
		}
		else
		{
			cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_morton_codes_offset, (int)scratchLayout.MortonCodes);
			cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_primitive_refs_offset, (int)scratchLayout.PrimitiveRefs);
		}
	}
}
