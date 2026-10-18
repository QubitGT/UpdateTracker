using Unity.Collections;
using Unity.Jobs;

namespace UnityEngine.Rendering;

internal class CPUDrawInstanceData
{
	private NativeParallelHashMap<RangeKey, int> m_RangeHash;

	private NativeList<DrawRange> m_DrawRanges;

	private NativeParallelHashMap<DrawKey, int> m_BatchHash;

	private NativeList<DrawBatch> m_DrawBatches;

	private NativeList<DrawInstance> m_DrawInstances;

	private NativeList<int> m_DrawInstanceIndices;

	private NativeList<int> m_DrawBatchIndices;

	private bool m_NeedsRebuild;

	public NativeList<DrawInstance> drawInstances => m_DrawInstances;

	public NativeParallelHashMap<DrawKey, int> batchHash => m_BatchHash;

	public NativeList<DrawBatch> drawBatches => m_DrawBatches;

	public NativeParallelHashMap<RangeKey, int> rangeHash => m_RangeHash;

	public NativeList<DrawRange> drawRanges => m_DrawRanges;

	public NativeArray<int> drawBatchIndices => m_DrawBatchIndices.AsArray();

	public NativeArray<int> drawInstanceIndices => m_DrawInstanceIndices.AsArray();

	public bool valid => m_DrawInstances.IsCreated;

	public void Initialize()
	{
		m_RangeHash = new NativeParallelHashMap<RangeKey, int>(1024, Allocator.Persistent);
		m_DrawRanges = new NativeList<DrawRange>(Allocator.Persistent);
		m_BatchHash = new NativeParallelHashMap<DrawKey, int>(1024, Allocator.Persistent);
		m_DrawBatches = new NativeList<DrawBatch>(Allocator.Persistent);
		m_DrawInstances = new NativeList<DrawInstance>(1024, Allocator.Persistent);
		m_DrawInstanceIndices = new NativeList<int>(1024, Allocator.Persistent);
		m_DrawBatchIndices = new NativeList<int>(1024, Allocator.Persistent);
	}

	public void Dispose()
	{
		if (m_DrawBatchIndices.IsCreated)
		{
			m_DrawBatchIndices.Dispose();
		}
		if (m_DrawInstanceIndices.IsCreated)
		{
			m_DrawInstanceIndices.Dispose();
		}
		if (m_DrawInstances.IsCreated)
		{
			m_DrawInstances.Dispose();
		}
		if (m_DrawBatches.IsCreated)
		{
			m_DrawBatches.Dispose();
		}
		if (m_BatchHash.IsCreated)
		{
			m_BatchHash.Dispose();
		}
		if (m_DrawRanges.IsCreated)
		{
			m_DrawRanges.Dispose();
		}
		if (m_RangeHash.IsCreated)
		{
			m_RangeHash.Dispose();
		}
	}

	public void RebuildDrawListsIfNeeded()
	{
		if (m_NeedsRebuild)
		{
			m_NeedsRebuild = false;
			m_DrawInstanceIndices.ResizeUninitialized(m_DrawInstances.Length);
			m_DrawBatchIndices.ResizeUninitialized(m_DrawBatches.Length);
			NativeArray<int> internalDrawIndex = new NativeArray<int>(drawBatches.Length * 16, Allocator.TempJob);
			PrefixSumDrawInstancesJob jobData = default(PrefixSumDrawInstancesJob);
			jobData.rangeHash = m_RangeHash;
			jobData.drawRanges = m_DrawRanges;
			jobData.drawBatches = m_DrawBatches;
			jobData.drawBatchIndices = m_DrawBatchIndices.AsArray();
			JobHandle dependsOn = jobData.Schedule();
			BuildDrawListsJob jobData2 = default(BuildDrawListsJob);
			jobData2.drawInstances = m_DrawInstances;
			jobData2.batchHash = m_BatchHash;
			jobData2.drawBatches = m_DrawBatches;
			jobData2.internalDrawIndex = internalDrawIndex;
			jobData2.drawInstanceIndices = m_DrawInstanceIndices.AsArray();
			IJobParallelForExtensions.Schedule(jobData2, m_DrawInstances.Length, 128, dependsOn).Complete();
			internalDrawIndex.Dispose();
		}
	}

	public void DestroyDrawInstanceIndices(NativeArray<int> drawInstanceIndicesToDestroy)
	{
		drawInstanceIndicesToDestroy.ParallelSort().Complete();
		InstanceCullingBatcherBurst.RemoveDrawInstanceIndices(in drawInstanceIndicesToDestroy, ref m_DrawInstances, ref m_RangeHash, ref m_BatchHash, ref m_DrawRanges, ref m_DrawBatches);
	}

	public void DestroyDrawInstances(NativeArray<InstanceHandle> destroyedInstances)
	{
		if (!m_DrawInstances.IsEmpty && destroyedInstances.Length != 0)
		{
			NeedsRebuild();
			NativeArray<InstanceHandle> instancesSorted = new NativeArray<InstanceHandle>(destroyedInstances, Allocator.TempJob);
			instancesSorted.Reinterpret<int>().ParallelSort().Complete();
			NativeList<int> nativeList = new NativeList<int>(m_DrawInstances.Length, Allocator.TempJob);
			FindDrawInstancesJob jobData = default(FindDrawInstancesJob);
			jobData.instancesSorted = instancesSorted;
			jobData.drawInstances = m_DrawInstances;
			jobData.outDrawInstanceIndicesWriter = nativeList.AsParallelWriter();
			jobData.ScheduleBatch(m_DrawInstances.Length, 128).Complete();
			DestroyDrawInstanceIndices(nativeList.AsArray());
			instancesSorted.Dispose();
			nativeList.Dispose();
		}
	}

	public void DestroyMaterialDrawInstances(NativeArray<uint> destroyedBatchMaterials)
	{
		if (!m_DrawInstances.IsEmpty && destroyedBatchMaterials.Length != 0)
		{
			NeedsRebuild();
			NativeArray<uint> materialsSorted = new NativeArray<uint>(destroyedBatchMaterials, Allocator.TempJob);
			materialsSorted.Reinterpret<int>().ParallelSort().Complete();
			NativeList<int> nativeList = new NativeList<int>(m_DrawInstances.Length, Allocator.TempJob);
			FindMaterialDrawInstancesJob jobData = default(FindMaterialDrawInstancesJob);
			jobData.materialsSorted = materialsSorted;
			jobData.drawInstances = m_DrawInstances;
			jobData.outDrawInstanceIndicesWriter = nativeList.AsParallelWriter();
			jobData.ScheduleBatch(m_DrawInstances.Length, 128).Complete();
			DestroyDrawInstanceIndices(nativeList.AsArray());
			materialsSorted.Dispose();
			nativeList.Dispose();
		}
	}

	public void NeedsRebuild()
	{
		m_NeedsRebuild = true;
	}
}
