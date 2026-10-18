using System;
using System.Runtime.InteropServices;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering;

public struct GPUPrefixSum
{
	[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\Utilities\\GPUPrefixSum\\GPUPrefixSum.Data.cs")]
	internal static class ShaderDefs
	{
		public const int GroupSize = 128;

		public const int ArgsBufferStride = 16;

		public const int ArgsBufferUpper = 0;

		public const int ArgsBufferLower = 8;

		public static int DivUpGroup(int value)
		{
			return (value + 128 - 1) / 128;
		}

		public static int AlignUpGroup(int value)
		{
			return DivUpGroup(value) * 128;
		}

		public static void CalculateTotalBufferSize(int maxElementCount, out int totalSize, out int levelCounts)
		{
			int num = (totalSize = AlignUpGroup(maxElementCount));
			levelCounts = 1;
			while (num > 128)
			{
				num = AlignUpGroup(DivUpGroup(num));
				totalSize += num;
				levelCounts++;
			}
		}
	}

	[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\Utilities\\GPUPrefixSum\\GPUPrefixSum.Data.cs")]
	public struct LevelOffsets
	{
		public uint count;

		public uint offset;

		public uint parentOffset;
	}

	public struct RenderGraphResources
	{
		internal int alignedElementCount;

		internal int maxBufferCount;

		internal int maxLevelCount;

		internal BufferHandle prefixBuffer0;

		internal BufferHandle prefixBuffer1;

		internal BufferHandle totalLevelCountBuffer;

		internal BufferHandle levelOffsetBuffer;

		internal BufferHandle indirectDispatchArgsBuffer;

		public BufferHandle output => prefixBuffer0;

		public static RenderGraphResources Create(int newMaxElementCount, RenderGraph renderGraph, RenderGraphBuilder builder, bool outputIsTemp = false)
		{
			RenderGraphResources result = default(RenderGraphResources);
			result.Initialize(newMaxElementCount, renderGraph, builder, outputIsTemp);
			return result;
		}

		private void Initialize(int newMaxElementCount, RenderGraph renderGraph, RenderGraphBuilder builder, bool outputIsTemp = false)
		{
			newMaxElementCount = Math.Max(newMaxElementCount, 1);
			ShaderDefs.CalculateTotalBufferSize(newMaxElementCount, out var totalSize, out var levelCounts);
			BufferDesc bufferDesc = new BufferDesc(totalSize, 4, GraphicsBuffer.Target.Raw);
			bufferDesc.name = "prefixBuffer0";
			BufferDesc desc = bufferDesc;
			BufferHandle bufferHandle;
			if (!outputIsTemp)
			{
				BufferHandle input = renderGraph.CreateBuffer(in desc);
				bufferHandle = builder.WriteBuffer(in input);
			}
			else
			{
				bufferHandle = builder.CreateTransientBuffer(in desc);
			}
			prefixBuffer0 = bufferHandle;
			bufferDesc = new BufferDesc(newMaxElementCount, 4, GraphicsBuffer.Target.Raw)
			{
				name = "prefixBuffer1"
			};
			prefixBuffer1 = builder.CreateTransientBuffer(in bufferDesc);
			bufferDesc = new BufferDesc(1, 4, GraphicsBuffer.Target.Raw)
			{
				name = "totalLevelCountBuffer"
			};
			totalLevelCountBuffer = builder.CreateTransientBuffer(in bufferDesc);
			bufferDesc = new BufferDesc(levelCounts, Marshal.SizeOf<LevelOffsets>(), GraphicsBuffer.Target.Structured)
			{
				name = "levelOffsetBuffer"
			};
			levelOffsetBuffer = builder.CreateTransientBuffer(in bufferDesc);
			bufferDesc = new BufferDesc(16 * levelCounts, 4, GraphicsBuffer.Target.Structured | GraphicsBuffer.Target.IndirectArguments)
			{
				name = "indirectDispatchArgsBuffer"
			};
			indirectDispatchArgsBuffer = builder.CreateTransientBuffer(in bufferDesc);
			alignedElementCount = ShaderDefs.AlignUpGroup(newMaxElementCount);
			maxBufferCount = totalSize;
			maxLevelCount = levelCounts;
		}
	}

	public struct SupportResources
	{
		internal bool ownsResources;

		internal int alignedElementCount;

		internal int maxBufferCount;

		internal int maxLevelCount;

		internal GraphicsBuffer prefixBuffer0;

		internal GraphicsBuffer prefixBuffer1;

		internal GraphicsBuffer totalLevelCountBuffer;

		internal GraphicsBuffer levelOffsetBuffer;

		internal GraphicsBuffer indirectDispatchArgsBuffer;

		public GraphicsBuffer output => prefixBuffer0;

		public static SupportResources Create(int maxElementCount)
		{
			SupportResources supportResources = default(SupportResources);
			supportResources.alignedElementCount = 0;
			supportResources.ownsResources = true;
			SupportResources result = supportResources;
			result.Resize(maxElementCount);
			return result;
		}

		public static SupportResources Load(RenderGraphResources shaderGraphResources)
		{
			SupportResources supportResources = default(SupportResources);
			supportResources.alignedElementCount = 0;
			supportResources.ownsResources = false;
			SupportResources result = supportResources;
			result.LoadFromShaderGraph(shaderGraphResources);
			return result;
		}

		internal void Resize(int newMaxElementCount)
		{
			if (!ownsResources)
			{
				throw new Exception("Cannot resize resources unless they are owned. Use GpuPrefixSumSupportResources.Create() for this.");
			}
			newMaxElementCount = Math.Max(newMaxElementCount, 1);
			if (alignedElementCount < newMaxElementCount)
			{
				Dispose();
				ShaderDefs.CalculateTotalBufferSize(newMaxElementCount, out var totalSize, out var levelCounts);
				alignedElementCount = ShaderDefs.AlignUpGroup(newMaxElementCount);
				maxBufferCount = totalSize;
				maxLevelCount = levelCounts;
				prefixBuffer0 = new GraphicsBuffer(GraphicsBuffer.Target.Raw, totalSize, 4);
				prefixBuffer1 = new GraphicsBuffer(GraphicsBuffer.Target.Raw, newMaxElementCount, 4);
				totalLevelCountBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, 1, 4);
				levelOffsetBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, levelCounts, Marshal.SizeOf<LevelOffsets>());
				indirectDispatchArgsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 16 * levelCounts, 4);
			}
		}

		private void LoadFromShaderGraph(RenderGraphResources shaderGraphResources)
		{
			alignedElementCount = shaderGraphResources.alignedElementCount;
			maxBufferCount = shaderGraphResources.maxBufferCount;
			maxLevelCount = shaderGraphResources.maxLevelCount;
			prefixBuffer0 = shaderGraphResources.prefixBuffer0;
			prefixBuffer1 = shaderGraphResources.prefixBuffer1;
			totalLevelCountBuffer = shaderGraphResources.totalLevelCountBuffer;
			levelOffsetBuffer = shaderGraphResources.levelOffsetBuffer;
			indirectDispatchArgsBuffer = shaderGraphResources.indirectDispatchArgsBuffer;
		}

		public void Dispose()
		{
			if (alignedElementCount != 0 && ownsResources)
			{
				alignedElementCount = 0;
				TryFreeBuffer(prefixBuffer0);
				TryFreeBuffer(prefixBuffer1);
				TryFreeBuffer(levelOffsetBuffer);
				TryFreeBuffer(indirectDispatchArgsBuffer);
				TryFreeBuffer(totalLevelCountBuffer);
			}
			static void TryFreeBuffer(GraphicsBuffer resource)
			{
				if (resource != null)
				{
					resource.Dispose();
					resource = null;
				}
			}
		}
	}

	public struct DirectArgs
	{
		public bool exclusive;

		public int inputCount;

		public GraphicsBuffer input;

		public SupportResources supportResources;
	}

	public struct IndirectDirectArgs
	{
		public bool exclusive;

		public int inputCountBufferByteOffset;

		public ComputeBuffer inputCountBuffer;

		public GraphicsBuffer input;

		public SupportResources supportResources;
	}

	public struct SystemResources
	{
		public ComputeShader computeAsset;

		internal int kernelCalculateLevelDispatchArgsFromConst;

		internal int kernelCalculateLevelDispatchArgsFromBuffer;

		internal int kernelPrefixSumOnGroup;

		internal int kernelPrefixSumOnGroupExclusive;

		internal int kernelPrefixSumNextInput;

		internal int kernelPrefixSumResolveParent;

		internal int kernelPrefixSumResolveParentExclusive;

		internal void LoadKernels()
		{
			if (!(computeAsset == null))
			{
				kernelCalculateLevelDispatchArgsFromConst = computeAsset.FindKernel("MainCalculateLevelDispatchArgsFromConst");
				kernelCalculateLevelDispatchArgsFromBuffer = computeAsset.FindKernel("MainCalculateLevelDispatchArgsFromBuffer");
				kernelPrefixSumOnGroup = computeAsset.FindKernel("MainPrefixSumOnGroup");
				kernelPrefixSumOnGroupExclusive = computeAsset.FindKernel("MainPrefixSumOnGroupExclusive");
				kernelPrefixSumNextInput = computeAsset.FindKernel("MainPrefixSumNextInput");
				kernelPrefixSumResolveParent = computeAsset.FindKernel("MainPrefixSumResolveParent");
				kernelPrefixSumResolveParentExclusive = computeAsset.FindKernel("MainPrefixSumResolveParentExclusive");
			}
		}
	}

	private static class ShaderIDs
	{
		public static readonly int _InputBuffer = Shader.PropertyToID("_InputBuffer");

		public static readonly int _OutputBuffer = Shader.PropertyToID("_OutputBuffer");

		public static readonly int _InputCountBuffer = Shader.PropertyToID("_InputCountBuffer");

		public static readonly int _TotalLevelsBuffer = Shader.PropertyToID("_TotalLevelsBuffer");

		public static readonly int _OutputTotalLevelsBuffer = Shader.PropertyToID("_OutputTotalLevelsBuffer");

		public static readonly int _OutputDispatchLevelArgsBuffer = Shader.PropertyToID("_OutputDispatchLevelArgsBuffer");

		public static readonly int _LevelsOffsetsBuffer = Shader.PropertyToID("_LevelsOffsetsBuffer");

		public static readonly int _OutputLevelsOffsetsBuffer = Shader.PropertyToID("_OutputLevelsOffsetsBuffer");

		public static readonly int _PrefixSumIntArgs = Shader.PropertyToID("_PrefixSumIntArgs");
	}

	private SystemResources resources;

	public GPUPrefixSum(SystemResources resources)
	{
		this.resources = resources;
		this.resources.LoadKernels();
	}

	private unsafe Vector4 PackPrefixSumArgs(int a, int b, int c, int d)
	{
		return new Vector4(*(float*)(&a), *(float*)(&b), *(float*)(&c), *(float*)(&d));
	}

	internal void ExecuteCommonIndirect(CommandBuffer cmdBuffer, GraphicsBuffer inputBuffer, in SupportResources supportResources, bool isExclusive)
	{
		int kernelIndex = (isExclusive ? resources.kernelPrefixSumOnGroupExclusive : resources.kernelPrefixSumOnGroup);
		int kernelIndex2 = (isExclusive ? resources.kernelPrefixSumResolveParentExclusive : resources.kernelPrefixSumResolveParent);
		for (int i = 0; i < supportResources.maxLevelCount; i++)
		{
			Vector4 val = PackPrefixSumArgs(0, 0, 0, i);
			cmdBuffer.SetComputeVectorParam(resources.computeAsset, ShaderIDs._PrefixSumIntArgs, val);
			if (i == 0)
			{
				cmdBuffer.SetComputeBufferParam(resources.computeAsset, kernelIndex, ShaderIDs._InputBuffer, inputBuffer);
			}
			else
			{
				cmdBuffer.SetComputeBufferParam(resources.computeAsset, kernelIndex, ShaderIDs._InputBuffer, supportResources.prefixBuffer1);
			}
			cmdBuffer.SetComputeBufferParam(resources.computeAsset, kernelIndex, ShaderIDs._TotalLevelsBuffer, supportResources.totalLevelCountBuffer);
			cmdBuffer.SetComputeBufferParam(resources.computeAsset, kernelIndex, ShaderIDs._LevelsOffsetsBuffer, supportResources.levelOffsetBuffer);
			cmdBuffer.SetComputeBufferParam(resources.computeAsset, kernelIndex, ShaderIDs._OutputBuffer, supportResources.prefixBuffer0);
			cmdBuffer.DispatchCompute(resources.computeAsset, kernelIndex, supportResources.indirectDispatchArgsBuffer, (uint)(i * 16 * 4));
			if (i != supportResources.maxLevelCount - 1)
			{
				cmdBuffer.SetComputeBufferParam(resources.computeAsset, resources.kernelPrefixSumNextInput, ShaderIDs._InputBuffer, supportResources.prefixBuffer0);
				cmdBuffer.SetComputeBufferParam(resources.computeAsset, resources.kernelPrefixSumNextInput, ShaderIDs._LevelsOffsetsBuffer, supportResources.levelOffsetBuffer);
				cmdBuffer.SetComputeBufferParam(resources.computeAsset, resources.kernelPrefixSumNextInput, ShaderIDs._OutputBuffer, supportResources.prefixBuffer1);
				cmdBuffer.DispatchCompute(resources.computeAsset, resources.kernelPrefixSumNextInput, supportResources.indirectDispatchArgsBuffer, (uint)((i + 1) * 16 * 4));
			}
		}
		for (int num = supportResources.maxLevelCount - 1; num >= 1; num--)
		{
			Vector4 val2 = PackPrefixSumArgs(0, 0, 0, num);
			cmdBuffer.SetComputeVectorParam(resources.computeAsset, ShaderIDs._PrefixSumIntArgs, val2);
			cmdBuffer.SetComputeBufferParam(resources.computeAsset, kernelIndex2, ShaderIDs._InputBuffer, inputBuffer);
			cmdBuffer.SetComputeBufferParam(resources.computeAsset, kernelIndex2, ShaderIDs._OutputBuffer, supportResources.prefixBuffer0);
			cmdBuffer.SetComputeBufferParam(resources.computeAsset, kernelIndex2, ShaderIDs._LevelsOffsetsBuffer, supportResources.levelOffsetBuffer);
			cmdBuffer.DispatchCompute(resources.computeAsset, kernelIndex2, supportResources.indirectDispatchArgsBuffer, (uint)(((num - 1) * 16 + 8) * 4));
		}
	}

	public void DispatchDirect(CommandBuffer cmdBuffer, in DirectArgs arguments)
	{
		if (arguments.supportResources.prefixBuffer0 == null || arguments.supportResources.prefixBuffer1 == null)
		{
			throw new Exception("Support resources are not valid.");
		}
		if (arguments.input == null)
		{
			throw new Exception("Input source buffer cannot be null.");
		}
		if (arguments.inputCount > arguments.supportResources.alignedElementCount)
		{
			throw new Exception("Input count exceeds maximum count of support resources. Ensure to create support resources with enough space.");
		}
		Vector4 val = PackPrefixSumArgs(arguments.inputCount, arguments.supportResources.maxLevelCount, 0, 0);
		cmdBuffer.SetComputeVectorParam(resources.computeAsset, ShaderIDs._PrefixSumIntArgs, val);
		cmdBuffer.SetComputeBufferParam(resources.computeAsset, resources.kernelCalculateLevelDispatchArgsFromConst, ShaderIDs._OutputLevelsOffsetsBuffer, arguments.supportResources.levelOffsetBuffer);
		cmdBuffer.SetComputeBufferParam(resources.computeAsset, resources.kernelCalculateLevelDispatchArgsFromConst, ShaderIDs._OutputDispatchLevelArgsBuffer, arguments.supportResources.indirectDispatchArgsBuffer);
		cmdBuffer.SetComputeBufferParam(resources.computeAsset, resources.kernelCalculateLevelDispatchArgsFromConst, ShaderIDs._OutputTotalLevelsBuffer, arguments.supportResources.totalLevelCountBuffer);
		cmdBuffer.DispatchCompute(resources.computeAsset, resources.kernelCalculateLevelDispatchArgsFromConst, 1, 1, 1);
		ExecuteCommonIndirect(cmdBuffer, arguments.input, in arguments.supportResources, arguments.exclusive);
	}

	public void DispatchIndirect(CommandBuffer cmdBuffer, in IndirectDirectArgs arguments)
	{
		if (arguments.supportResources.prefixBuffer0 == null || arguments.supportResources.prefixBuffer1 == null)
		{
			throw new Exception("Support resources are not valid.");
		}
		if (arguments.input == null || arguments.inputCountBuffer == null)
		{
			throw new Exception("Input source buffer and inputCountBuffer cannot be null.");
		}
		Vector4 val = PackPrefixSumArgs(0, arguments.supportResources.maxLevelCount, arguments.inputCountBufferByteOffset, 0);
		cmdBuffer.SetComputeVectorParam(resources.computeAsset, ShaderIDs._PrefixSumIntArgs, val);
		cmdBuffer.SetComputeBufferParam(resources.computeAsset, resources.kernelCalculateLevelDispatchArgsFromBuffer, ShaderIDs._InputCountBuffer, arguments.inputCountBuffer);
		cmdBuffer.SetComputeBufferParam(resources.computeAsset, resources.kernelCalculateLevelDispatchArgsFromBuffer, ShaderIDs._OutputLevelsOffsetsBuffer, arguments.supportResources.levelOffsetBuffer);
		cmdBuffer.SetComputeBufferParam(resources.computeAsset, resources.kernelCalculateLevelDispatchArgsFromBuffer, ShaderIDs._OutputDispatchLevelArgsBuffer, arguments.supportResources.indirectDispatchArgsBuffer);
		cmdBuffer.SetComputeBufferParam(resources.computeAsset, resources.kernelCalculateLevelDispatchArgsFromBuffer, ShaderIDs._OutputTotalLevelsBuffer, arguments.supportResources.totalLevelCountBuffer);
		cmdBuffer.DispatchCompute(resources.computeAsset, resources.kernelCalculateLevelDispatchArgsFromBuffer, 1, 1, 1);
		ExecuteCommonIndirect(cmdBuffer, arguments.input, in arguments.supportResources, arguments.exclusive);
	}
}
