using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace UnityEngine.Rendering.RadeonRays;

internal class HlbvhTopLevelBuilder
{
	private struct ScratchBufferLayout
	{
		public uint Aabb;

		public uint MortonCodes;

		public uint PrimitiveRefs;

		public uint SortedMortonCodes;

		public uint SortedPrimitiveRefs;

		public uint SortMemory;

		public uint InternalNodeRange;

		public uint TotalSize;

		public static ScratchBufferLayout Create(uint instanceCount)
		{
			ScratchBufferLayout result = default(ScratchBufferLayout);
			result.Aabb = result.Reserve(6u);
			result.MortonCodes = result.Reserve(instanceCount);
			result.PrimitiveRefs = result.Reserve(instanceCount);
			result.SortedMortonCodes = result.Reserve(instanceCount);
			result.SortedPrimitiveRefs = result.Reserve(instanceCount);
			result.SortMemory = result.Reserve((uint)RadixSort.GetScratchDataSizeInDwords(instanceCount));
			result.InternalNodeRange = result.MortonCodes;
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

	public HlbvhTopLevelBuilder(RadeonRaysShaders shaders)
	{
		shaderBuildHlbvh = shaders.buildHlbvh;
		kernelInit = shaderBuildHlbvh.FindKernel("Init");
		kernelCalculateAabb = shaderBuildHlbvh.FindKernel("CalculateAabb");
		kernelCalculateMortonCodes = shaderBuildHlbvh.FindKernel("CalculateMortonCodes");
		kernelBuildTreeBottomUp = shaderBuildHlbvh.FindKernel("BuildTreeBottomUp");
		radixSort = new RadixSort(shaders);
	}

	public ulong GetScratchDataSizeInDwords(uint instanceCount)
	{
		return ScratchBufferLayout.Create(instanceCount).TotalSize;
	}

	public static uint GetBvhNodeCount(uint leafCount)
	{
		return leafCount - 1;
	}

	public void AllocateResultBuffers(uint instanceCount, ref TopLevelAccelStruct accelStruct)
	{
		uint bvhNodeCount = GetBvhNodeCount(instanceCount);
		accelStruct.Dispose();
		accelStruct.instanceInfos = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)instanceCount, Marshal.SizeOf<InstanceInfo>());
		accelStruct.topLevelBvh = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)(bvhNodeCount + 1), Marshal.SizeOf<BvhNode>());
	}

	public void CreateEmpty(ref TopLevelAccelStruct accelStruct)
	{
		accelStruct.Dispose();
		accelStruct.topLevelBvh = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 2, Marshal.SizeOf<BvhNode>());
		accelStruct.instanceInfos = accelStruct.topLevelBvh;
		accelStruct.bottomLevelBvhs = accelStruct.topLevelBvh;
		accelStruct.instanceCount = 0u;
		BvhNode[] array = new BvhNode[2];
		array[0].child0 = 0u;
		array[0].child1 = 0u;
		array[0].parent = 0u;
		array[1].child0 = 0u;
		array[1].child1 = 0u;
		array[1].parent = uint.MaxValue;
		array[1].update = 0u;
		array[1].aabb0_min = new float3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
		array[1].aabb0_max = new float3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
		array[1].aabb1_min = new float3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
		array[1].aabb1_max = new float3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
		accelStruct.topLevelBvh.SetData(array);
	}

	public void Execute(CommandBuffer cmd, GraphicsBuffer scratch, ref TopLevelAccelStruct accelStruct)
	{
		Common.EnableKeyword(cmd, shaderBuildHlbvh, "TOP_LEVEL", enable: true);
		Common.EnableKeyword(cmd, shaderBuildHlbvh, "UINT16_INDICES", enable: false);
		uint instanceCount = accelStruct.instanceCount;
		ScratchBufferLayout scratchLayout = ScratchBufferLayout.Create(instanceCount);
		cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_constants_vertex_stride, 0);
		cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_constants_triangle_count, (int)instanceCount);
		cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_bvh_offset, 0);
		cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_internal_node_range_offset, (int)scratchLayout.InternalNodeRange);
		cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_aabb_offset, (int)scratchLayout.Aabb);
		BindKernelArguments(cmd, kernelInit, scratch, scratchLayout, accelStruct, setSortedCodes: false);
		cmd.DispatchCompute(shaderBuildHlbvh, kernelInit, 1, 1, 1);
		BindKernelArguments(cmd, kernelCalculateAabb, scratch, scratchLayout, accelStruct, setSortedCodes: false);
		cmd.DispatchCompute(shaderBuildHlbvh, kernelCalculateAabb, (int)Common.CeilDivide(instanceCount, 2048u), 1, 1);
		BindKernelArguments(cmd, kernelCalculateMortonCodes, scratch, scratchLayout, accelStruct, setSortedCodes: false);
		cmd.DispatchCompute(shaderBuildHlbvh, kernelCalculateMortonCodes, (int)Common.CeilDivide(instanceCount, 2048u), 1, 1);
		radixSort.Execute(cmd, scratch, scratchLayout.MortonCodes, scratchLayout.SortedMortonCodes, scratchLayout.PrimitiveRefs, scratchLayout.SortedPrimitiveRefs, scratchLayout.SortMemory, instanceCount);
		BindKernelArguments(cmd, kernelBuildTreeBottomUp, scratch, scratchLayout, accelStruct, setSortedCodes: true);
		cmd.DispatchCompute(shaderBuildHlbvh, kernelBuildTreeBottomUp, (int)Common.CeilDivide(instanceCount, 2048u), 1, 1);
	}

	private void BindKernelArguments(CommandBuffer cmd, int kernel, GraphicsBuffer scratch, ScratchBufferLayout scratchLayout, TopLevelAccelStruct accelStruct, bool setSortedCodes)
	{
		cmd.SetComputeBufferParam(shaderBuildHlbvh, kernel, SID.g_scratch_buffer, scratch);
		cmd.SetComputeBufferParam(shaderBuildHlbvh, kernel, SID.g_bvh, accelStruct.topLevelBvh);
		cmd.SetComputeBufferParam(shaderBuildHlbvh, kernel, SID.g_bottom_bvhs, accelStruct.bottomLevelBvhs);
		cmd.SetComputeBufferParam(shaderBuildHlbvh, kernel, SID.g_instance_infos, accelStruct.instanceInfos);
		cmd.SetComputeIntParam(shaderBuildHlbvh, SID.g_aabb_offset, (int)scratchLayout.Aabb);
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
