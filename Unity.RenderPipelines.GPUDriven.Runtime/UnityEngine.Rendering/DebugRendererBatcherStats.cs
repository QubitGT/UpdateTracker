using System;
using Unity.Collections;

namespace UnityEngine.Rendering;

internal class DebugRendererBatcherStats : IDisposable
{
	public bool enabled;

	public NativeList<InstanceCullerViewStats> instanceCullerStats;

	public NativeList<InstanceOcclusionEventStats> instanceOcclusionEventStats;

	public NativeList<DebugOccluderStats> occluderStats;

	public bool occlusionOverlayEnabled;

	public bool occlusionOverlayCountVisible;

	public bool overrideOcclusionTestToAlwaysPass;

	public DebugRendererBatcherStats()
	{
		instanceCullerStats = new NativeList<InstanceCullerViewStats>(Allocator.Persistent);
		instanceOcclusionEventStats = new NativeList<InstanceOcclusionEventStats>(Allocator.Persistent);
		occluderStats = new NativeList<DebugOccluderStats>(Allocator.Persistent);
	}

	public void FinalizeInstanceCullerViewStats()
	{
		for (int i = 0; i < instanceCullerStats.Length; i++)
		{
			InstanceCullerViewStats value = instanceCullerStats[i];
			InstanceOcclusionEventStats lastInstanceOcclusionEventStatsForView = GetLastInstanceOcclusionEventStatsForView(i);
			if (lastInstanceOcclusionEventStatsForView.viewInstanceID == value.viewInstanceID)
			{
				value.visibleInstancesOnGPU = Math.Min(lastInstanceOcclusionEventStatsForView.visibleInstances, value.visibleInstancesOnCPU);
				value.visiblePrimitivesOnGPU = Math.Min(lastInstanceOcclusionEventStatsForView.visiblePrimitives, value.visiblePrimitivesOnCPU);
			}
			else
			{
				value.visibleInstancesOnGPU = value.visibleInstancesOnCPU;
				value.visiblePrimitivesOnGPU = value.visiblePrimitivesOnCPU;
			}
			instanceCullerStats[i] = value;
		}
	}

	private InstanceOcclusionEventStats GetLastInstanceOcclusionEventStatsForView(int viewIndex)
	{
		if (viewIndex < instanceCullerStats.Length)
		{
			int viewInstanceID = instanceCullerStats[viewIndex].viewInstanceID;
			for (int num = instanceOcclusionEventStats.Length - 1; num >= 0; num--)
			{
				if (instanceOcclusionEventStats[num].viewInstanceID == viewInstanceID)
				{
					return instanceOcclusionEventStats[num];
				}
			}
		}
		return default(InstanceOcclusionEventStats);
	}

	public void Dispose()
	{
		if (instanceCullerStats.IsCreated)
		{
			instanceCullerStats.Dispose();
		}
		if (instanceOcclusionEventStats.IsCreated)
		{
			instanceOcclusionEventStats.Dispose();
		}
		if (occluderStats.IsCreated)
		{
			occluderStats.Dispose();
		}
	}
}
