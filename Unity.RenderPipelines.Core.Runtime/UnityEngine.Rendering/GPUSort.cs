using System;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering;

public struct GPUSort
{
	private enum Stage
	{
		LocalBMS,
		LocalDisperse,
		BigFlip,
		BigDisperse
	}

	public struct Args
	{
		public uint count;

		public uint maxDepth;

		public GraphicsBuffer inputKeys;

		public GraphicsBuffer inputValues;

		public SupportResources resources;

		internal int workGroupCount;
	}

	public struct RenderGraphResources
	{
		public BufferHandle sortBufferKeys;

		public BufferHandle sortBufferValues;

		public static RenderGraphResources Create(int count, RenderGraph renderGraph, RenderGraphBuilder builder)
		{
			GraphicsBuffer.Target target = GraphicsBuffer.Target.CopyDestination | GraphicsBuffer.Target.Raw;
			RenderGraphResources result = default(RenderGraphResources);
			BufferDesc desc = new BufferDesc(count, 4, target)
			{
				name = "Keys"
			};
			result.sortBufferKeys = builder.CreateTransientBuffer(in desc);
			BufferDesc desc2 = new BufferDesc(count, 4, target)
			{
				name = "Values"
			};
			result.sortBufferValues = builder.CreateTransientBuffer(in desc2);
			return result;
		}
	}

	public struct SupportResources
	{
		public GraphicsBuffer sortBufferKeys;

		public GraphicsBuffer sortBufferValues;

		public static SupportResources Load(RenderGraphResources renderGraphResources)
		{
			SupportResources result = default(SupportResources);
			result.sortBufferKeys = renderGraphResources.sortBufferKeys;
			result.sortBufferValues = renderGraphResources.sortBufferValues;
			return result;
		}

		public void Dispose()
		{
			if (sortBufferKeys != null)
			{
				sortBufferKeys.Dispose();
				sortBufferKeys = null;
			}
			if (sortBufferValues != null)
			{
				sortBufferValues.Dispose();
				sortBufferValues = null;
			}
		}
	}

	public struct SystemResources
	{
		public ComputeShader computeAsset;
	}

	private const uint kWorkGroupSize = 1024u;

	private LocalKeyword[] m_Keywords;

	private SystemResources resources;

	public GPUSort(SystemResources resources)
	{
		this.resources = resources;
		m_Keywords = new LocalKeyword[4]
		{
			new LocalKeyword(resources.computeAsset, "STAGE_BMS"),
			new LocalKeyword(resources.computeAsset, "STAGE_LOCAL_DISPERSE"),
			new LocalKeyword(resources.computeAsset, "STAGE_BIG_FLIP"),
			new LocalKeyword(resources.computeAsset, "STAGE_BIG_DISPERSE")
		};
	}

	private void DispatchStage(CommandBuffer cmd, Args args, uint h, Stage stage)
	{
		using (new ProfilingScope(cmd, ProfilingSampler.Get(stage)))
		{
			LocalKeyword[] keywords = m_Keywords;
			for (int i = 0; i < keywords.Length; i++)
			{
				LocalKeyword keyword = keywords[i];
				cmd.SetKeyword(resources.computeAsset, in keyword, value: false);
			}
			cmd.SetKeyword(resources.computeAsset, in m_Keywords[(int)stage], value: true);
			cmd.SetComputeIntParam(resources.computeAsset, "_H", (int)h);
			cmd.SetComputeIntParam(resources.computeAsset, "_Total", (int)args.count);
			cmd.SetComputeBufferParam(resources.computeAsset, 0, "_KeyBuffer", args.resources.sortBufferKeys);
			cmd.SetComputeBufferParam(resources.computeAsset, 0, "_ValueBuffer", args.resources.sortBufferValues);
			cmd.DispatchCompute(resources.computeAsset, 0, args.workGroupCount, 1, 1);
		}
	}

	private void CopyBuffer(CommandBuffer cmd, GraphicsBuffer src, GraphicsBuffer dst)
	{
		LocalKeyword[] keywords = m_Keywords;
		for (int i = 0; i < keywords.Length; i++)
		{
			LocalKeyword keyword = keywords[i];
			cmd.SetKeyword(resources.computeAsset, in keyword, value: false);
		}
		int num = src.count * src.stride / 4;
		cmd.SetComputeBufferParam(resources.computeAsset, 1, "_CopySrcBuffer", src);
		cmd.SetComputeBufferParam(resources.computeAsset, 1, "_CopyDstBuffer", dst);
		cmd.SetComputeIntParam(resources.computeAsset, "_CopyEntriesCount", num);
		cmd.DispatchCompute(resources.computeAsset, 1, (num + 63) / 64, 1, 1);
	}

	internal static int DivRoundUp(int x, int y)
	{
		return (x + y - 1) / y;
	}

	public void Dispatch(CommandBuffer cmd, Args args)
	{
		uint count = args.count;
		CopyBuffer(cmd, args.inputKeys, args.resources.sortBufferKeys);
		CopyBuffer(cmd, args.inputValues, args.resources.sortBufferValues);
		args.workGroupCount = Math.Max(1, DivRoundUp((int)count, 2048));
		uint num = Math.Min(2048u, args.maxDepth);
		DispatchStage(cmd, args, num, Stage.LocalBMS);
		for (num *= 2; num <= Math.Min(count, args.maxDepth); num *= 2)
		{
			DispatchStage(cmd, args, num, Stage.BigFlip);
			for (uint num2 = num / 2; num2 > 1; num2 /= 2)
			{
				if (num2 <= 2048)
				{
					DispatchStage(cmd, args, num2, Stage.LocalDisperse);
					break;
				}
				DispatchStage(cmd, args, num2, Stage.BigDisperse);
			}
		}
	}
}
