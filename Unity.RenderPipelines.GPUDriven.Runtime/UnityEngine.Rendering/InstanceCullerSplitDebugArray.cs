using System;
using Unity.Collections;
using Unity.Jobs;

namespace UnityEngine.Rendering;

internal struct InstanceCullerSplitDebugArray : IDisposable
{
	internal struct Info
	{
		public BatchCullingViewType viewType;

		public int viewInstanceID;

		public int splitIndex;
	}

	private const int MaxSplitCount = 64;

	private NativeList<Info> m_Info;

	private NativeArray<int> m_Counters;

	private NativeQueue<JobHandle> m_CounterSync;

	public NativeArray<int> Counters => m_Counters;

	public void Init()
	{
		m_Info = new NativeList<Info>(Allocator.Persistent);
		m_Counters = new NativeArray<int>(192, Allocator.Persistent);
		m_CounterSync = new NativeQueue<JobHandle>(Allocator.Persistent);
	}

	public void Dispose()
	{
		m_Info.Dispose();
		m_Counters.Dispose();
		m_CounterSync.Dispose();
	}

	public int TryAddSplits(BatchCullingViewType viewType, int viewInstanceID, int splitCount)
	{
		int length = m_Info.Length;
		if (length + splitCount > 64)
		{
			return -1;
		}
		for (int i = 0; i < splitCount; i++)
		{
			ref NativeList<Info> info = ref m_Info;
			Info value = new Info
			{
				viewType = viewType,
				viewInstanceID = viewInstanceID,
				splitIndex = i
			};
			info.Add(in value);
		}
		return length;
	}

	public void AddSync(int baseIndex, JobHandle jobHandle)
	{
		if (baseIndex != -1)
		{
			m_CounterSync.Enqueue(jobHandle);
		}
	}

	public void MoveToDebugStatsAndClear(DebugRendererBatcherStats debugStats)
	{
		JobHandle item;
		while (m_CounterSync.TryDequeue(out item))
		{
			item.Complete();
		}
		debugStats.instanceCullerStats.Clear();
		for (int i = 0; i < m_Info.Length; i++)
		{
			Info info = m_Info[i];
			int num = i * 3;
			ref NativeList<InstanceCullerViewStats> instanceCullerStats = ref debugStats.instanceCullerStats;
			InstanceCullerViewStats value = new InstanceCullerViewStats
			{
				viewType = info.viewType,
				viewInstanceID = info.viewInstanceID,
				splitIndex = info.splitIndex,
				visibleInstancesOnCPU = m_Counters[num],
				visibleInstancesOnGPU = 0,
				visiblePrimitivesOnCPU = m_Counters[num + 1],
				visiblePrimitivesOnGPU = 0,
				drawCommands = m_Counters[num + 2]
			};
			instanceCullerStats.Add(in value);
		}
		m_Info.Clear();
		ref NativeArray<int> counters = ref m_Counters;
		int value2 = 0;
		counters.FillArray(in value2);
	}
}
