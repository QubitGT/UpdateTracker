using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering;

internal struct IndirectBufferContextStorage : IDisposable
{
	private const int kAllocatorCount = 2;

	internal const int kExtraDrawAllocationCount = 1;

	internal const int kInstanceInfoGpuOffsetMultiplier = 2;

	private IndirectBufferLimits m_BufferLimits;

	private GraphicsBuffer m_InstanceBuffer;

	private GraphicsBuffer m_InstanceInfoBuffer;

	private NativeArray<IndirectInstanceInfo> m_InstanceInfoStaging;

	private GraphicsBuffer m_ArgsBuffer;

	private GraphicsBuffer m_DrawInfoBuffer;

	private NativeArray<IndirectDrawInfo> m_DrawInfoStaging;

	private int m_ContextAllocCounter;

	private NativeHashMap<int, int> m_ContextIndexFromViewID;

	private NativeList<IndirectBufferContext> m_Contexts;

	private NativeArray<IndirectBufferAllocInfo> m_ContextAllocInfo;

	private NativeArray<int> m_AllocationCounters;

	public GraphicsBuffer instanceBuffer => m_InstanceBuffer;

	public GraphicsBuffer instanceInfoBuffer => m_InstanceInfoBuffer;

	public GraphicsBuffer argsBuffer => m_ArgsBuffer;

	public GraphicsBuffer drawInfoBuffer => m_DrawInfoBuffer;

	public GraphicsBufferHandle visibleInstanceBufferHandle => m_InstanceBuffer.bufferHandle;

	public GraphicsBufferHandle indirectArgsBufferHandle => m_ArgsBuffer.bufferHandle;

	public NativeArray<IndirectInstanceInfo> instanceInfoGlobalArray => m_InstanceInfoStaging;

	public NativeArray<IndirectDrawInfo> drawInfoGlobalArray => m_DrawInfoStaging;

	public NativeArray<int> allocationCounters => m_AllocationCounters;

	public IndirectBufferContextHandles ImportBuffers(RenderGraph renderGraph)
	{
		IndirectBufferContextHandles result = default(IndirectBufferContextHandles);
		result.instanceBuffer = renderGraph.ImportBuffer(m_InstanceBuffer);
		result.instanceInfoBuffer = renderGraph.ImportBuffer(m_InstanceInfoBuffer);
		result.argsBuffer = renderGraph.ImportBuffer(m_ArgsBuffer);
		result.drawInfoBuffer = renderGraph.ImportBuffer(m_DrawInfoBuffer);
		return result;
	}

	public void Init()
	{
		int num = 256;
		int maxInstanceCount = 64 * num;
		int num2 = 8;
		AllocateInstanceBuffers(maxInstanceCount);
		AllocateDrawBuffers(num);
		m_ContextIndexFromViewID = new NativeHashMap<int, int>(num2, Allocator.Persistent);
		m_Contexts = new NativeList<IndirectBufferContext>(num2, Allocator.Persistent);
		m_ContextAllocInfo = new NativeArray<IndirectBufferAllocInfo>(num2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_AllocationCounters = new NativeArray<int>(2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ResetAllocators();
	}

	private void AllocateInstanceBuffers(int maxInstanceCount)
	{
		m_InstanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, maxInstanceCount, 4);
		m_InstanceInfoBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 2 * maxInstanceCount, Marshal.SizeOf<IndirectInstanceInfo>());
		m_InstanceInfoStaging = new NativeArray<IndirectInstanceInfo>(maxInstanceCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_BufferLimits.maxInstanceCount = maxInstanceCount;
	}

	private void FreeInstanceBuffers()
	{
		m_InstanceBuffer.Release();
		m_InstanceInfoBuffer.Release();
		m_InstanceInfoStaging.Dispose();
		m_BufferLimits.maxInstanceCount = 0;
	}

	private void AllocateDrawBuffers(int maxDrawCount)
	{
		m_ArgsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured | GraphicsBuffer.Target.IndirectArguments, (maxDrawCount + 1) * 5, 4);
		m_DrawInfoBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxDrawCount, Marshal.SizeOf<IndirectDrawInfo>());
		m_DrawInfoStaging = new NativeArray<IndirectDrawInfo>(maxDrawCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_BufferLimits.maxDrawCount = maxDrawCount;
	}

	private void FreeDrawBuffers()
	{
		m_ArgsBuffer.Release();
		m_DrawInfoBuffer.Release();
		m_DrawInfoStaging.Dispose();
		m_BufferLimits.maxDrawCount = 0;
	}

	public void Dispose()
	{
		SyncContexts();
		FreeInstanceBuffers();
		FreeDrawBuffers();
		m_ContextIndexFromViewID.Dispose();
		m_Contexts.Dispose();
		m_ContextAllocInfo.Dispose();
		m_AllocationCounters.Dispose();
	}

	private void SyncContexts()
	{
		for (int i = 0; i < m_Contexts.Length; i++)
		{
			m_Contexts[i].cullingJobHandle.Complete();
		}
	}

	private void ResetAllocators()
	{
		m_ContextAllocCounter = 0;
		m_ContextIndexFromViewID.Clear();
		m_Contexts.Clear();
		ref NativeArray<int> array = ref m_AllocationCounters;
		int value = 0;
		array.FillArray(in value);
	}

	private void GrowBuffers()
	{
		if (m_ContextAllocCounter > m_ContextAllocInfo.Length)
		{
			int num = m_ContextAllocCounter * 6 / 5;
			m_Contexts.Clear();
			m_Contexts.SetCapacity(num);
			m_ContextAllocInfo.Dispose();
			m_ContextAllocInfo = new NativeArray<IndirectBufferAllocInfo>(num, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}
		int num2 = m_AllocationCounters[0];
		if (num2 > m_BufferLimits.maxInstanceCount)
		{
			int maxInstanceCount = num2 * 6 / 5;
			FreeInstanceBuffers();
			AllocateInstanceBuffers(maxInstanceCount);
		}
		int num3 = m_AllocationCounters[1];
		if (num3 > m_BufferLimits.maxDrawCount)
		{
			int maxDrawCount = num3 * 6 / 5;
			FreeDrawBuffers();
			AllocateDrawBuffers(maxDrawCount);
		}
	}

	public void ClearContextsAndGrowBuffers()
	{
		SyncContexts();
		GrowBuffers();
		ResetAllocators();
	}

	public int TryAllocateContext(int viewID)
	{
		if (m_ContextIndexFromViewID.ContainsKey(viewID))
		{
			return -1;
		}
		int num = -1;
		m_ContextAllocCounter++;
		if (m_Contexts.Length < m_ContextAllocInfo.Length)
		{
			num = m_Contexts.Length;
			ref NativeList<IndirectBufferContext> contexts = ref m_Contexts;
			IndirectBufferContext value = default(IndirectBufferContext);
			contexts.Add(in value);
			m_ContextIndexFromViewID.Add(viewID, num);
		}
		return num;
	}

	public int TryGetContextIndex(int viewID)
	{
		if (!m_ContextIndexFromViewID.TryGetValue(viewID, out var item))
		{
			return -1;
		}
		return item;
	}

	public NativeArray<IndirectBufferAllocInfo> GetAllocInfoSubArray(int contextIndex)
	{
		int start = Mathf.Max(contextIndex, 0);
		return m_ContextAllocInfo.GetSubArray(start, 1);
	}

	public IndirectBufferAllocInfo GetAllocInfo(int contextIndex)
	{
		IndirectBufferAllocInfo result = default(IndirectBufferAllocInfo);
		if (0 <= contextIndex && contextIndex < m_Contexts.Length)
		{
			return m_ContextAllocInfo[contextIndex];
		}
		return result;
	}

	public void CopyFromStaging(CommandBuffer cmd, in IndirectBufferAllocInfo allocInfo)
	{
		if (!allocInfo.IsEmpty())
		{
			cmd.SetBufferData(m_DrawInfoBuffer, m_DrawInfoStaging, allocInfo.drawAllocIndex, allocInfo.drawAllocIndex, allocInfo.drawCount);
			cmd.SetBufferData(m_InstanceInfoBuffer, m_InstanceInfoStaging, allocInfo.instanceAllocIndex, 2 * allocInfo.instanceAllocIndex, allocInfo.instanceCount);
		}
	}

	public IndirectBufferLimits GetLimits(int contextIndex)
	{
		IndirectBufferLimits result = default(IndirectBufferLimits);
		if (contextIndex >= 0)
		{
			return m_BufferLimits;
		}
		return result;
	}

	public IndirectBufferContext GetBufferContext(int contextIndex)
	{
		IndirectBufferContext result = default(IndirectBufferContext);
		if (0 <= contextIndex && contextIndex < m_Contexts.Length)
		{
			return m_Contexts[contextIndex];
		}
		return result;
	}

	public void SetBufferContext(int contextIndex, IndirectBufferContext ctx)
	{
		if (0 <= contextIndex && contextIndex < m_Contexts.Length)
		{
			m_Contexts[contextIndex] = ctx;
		}
	}
}
