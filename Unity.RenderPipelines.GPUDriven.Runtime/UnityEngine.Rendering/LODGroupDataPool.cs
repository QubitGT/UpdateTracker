using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UnityEngine.Rendering;

internal class LODGroupDataPool : IDisposable
{
	private static class LodGroupShaderIDs
	{
		public static readonly int _SupportDitheringCrossFade = Shader.PropertyToID("_SupportDitheringCrossFade");

		public static readonly int _LodGroupCullingDataGPUByteSize = Shader.PropertyToID("_LodGroupCullingDataGPUByteSize");

		public static readonly int _LodGroupCullingDataStartOffset = Shader.PropertyToID("_LodGroupCullingDataStartOffset");

		public static readonly int _LodCullingDataQueueCount = Shader.PropertyToID("_LodCullingDataQueueCount");

		public static readonly int _InputLodCullingDataIndices = Shader.PropertyToID("_InputLodCullingDataIndices");

		public static readonly int _InputLodCullingDataBuffer = Shader.PropertyToID("_InputLodCullingDataBuffer");

		public static readonly int _LodGroupCullingData = Shader.PropertyToID("_LodGroupCullingData");
	}

	private NativeList<LODGroupData> m_LODGroupData;

	private NativeParallelHashMap<int, GPUInstanceIndex> m_LODGroupDataHash;

	private NativeList<LODGroupCullingData> m_LODGroupCullingData;

	private NativeList<GPUInstanceIndex> m_FreeLODGroupDataHandles;

	private int m_CrossfadedRendererCount;

	private bool m_SupportDitheringCrossFade;

	public NativeParallelHashMap<int, GPUInstanceIndex> lodGroupDataHash => m_LODGroupDataHash;

	public NativeList<LODGroupCullingData> lodGroupCullingData => m_LODGroupCullingData;

	public int crossfadedRendererCount => m_CrossfadedRendererCount;

	public int activeLodGroupCount => m_LODGroupData.Length;

	public LODGroupDataPool(GPUResidentDrawerResources resources, int initialInstanceCount, bool supportDitheringCrossFade)
	{
		m_LODGroupData = new NativeList<LODGroupData>(Allocator.Persistent);
		m_LODGroupDataHash = new NativeParallelHashMap<int, GPUInstanceIndex>(64, Allocator.Persistent);
		m_LODGroupCullingData = new NativeList<LODGroupCullingData>(Allocator.Persistent);
		m_FreeLODGroupDataHandles = new NativeList<GPUInstanceIndex>(Allocator.Persistent);
		m_SupportDitheringCrossFade = supportDitheringCrossFade;
	}

	public void Dispose()
	{
		m_LODGroupData.Dispose();
		m_LODGroupDataHash.Dispose();
		m_LODGroupCullingData.Dispose();
		m_FreeLODGroupDataHandles.Dispose();
	}

	public unsafe void UpdateLODGroupTransformData(in GPUDrivenLODGroupData inputData)
	{
		int length = inputData.lodGroupID.Length;
		int num = 0;
		UpdateLODGroupTransformJob updateLODGroupTransformJob = default(UpdateLODGroupTransformJob);
		updateLODGroupTransformJob.lodGroupDataHash = m_LODGroupDataHash;
		updateLODGroupTransformJob.lodGroupIDs = inputData.lodGroupID;
		updateLODGroupTransformJob.worldSpaceReferencePoints = inputData.worldSpaceReferencePoint;
		updateLODGroupTransformJob.worldSpaceSizes = inputData.worldSpaceSize;
		updateLODGroupTransformJob.lodGroupData = m_LODGroupData;
		updateLODGroupTransformJob.lodGroupCullingData = m_LODGroupCullingData;
		updateLODGroupTransformJob.supportDitheringCrossFade = m_SupportDitheringCrossFade;
		updateLODGroupTransformJob.atomicUpdateCount = new UnsafeAtomicCounter32(&num);
		UpdateLODGroupTransformJob jobData = updateLODGroupTransformJob;
		if (length >= 256)
		{
			IJobParallelForExtensions.Schedule(jobData, length, 256).Complete();
		}
		else
		{
			IJobParallelForExtensions.Run(jobData, length);
		}
	}

	public unsafe void UpdateLODGroupData(in GPUDrivenLODGroupData inputData)
	{
		FreeLODGroupData(inputData.invalidLODGroupID);
		NativeArray<GPUInstanceIndex> lodGroupInstances = new NativeArray<GPUInstanceIndex>(inputData.lodGroupID.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		int num = LODGroupDataPoolBurst.AllocateOrGetLODGroupDataInstances(in inputData.lodGroupID, ref m_LODGroupData, ref m_LODGroupCullingData, ref m_LODGroupDataHash, ref m_FreeLODGroupDataHandles, ref lodGroupInstances);
		m_CrossfadedRendererCount -= num;
		int num2 = 0;
		UpdateLODGroupDataJob updateLODGroupDataJob = default(UpdateLODGroupDataJob);
		updateLODGroupDataJob.lodGroupInstances = lodGroupInstances;
		updateLODGroupDataJob.inputData = inputData;
		updateLODGroupDataJob.supportDitheringCrossFade = m_SupportDitheringCrossFade;
		updateLODGroupDataJob.lodGroupsData = m_LODGroupData.AsArray();
		updateLODGroupDataJob.lodGroupsCullingData = m_LODGroupCullingData.AsArray();
		updateLODGroupDataJob.rendererCount = new UnsafeAtomicCounter32(&num2);
		UpdateLODGroupDataJob jobData = updateLODGroupDataJob;
		if (lodGroupInstances.Length >= 256)
		{
			IJobParallelForExtensions.Schedule(jobData, lodGroupInstances.Length, 256).Complete();
		}
		else
		{
			IJobParallelForExtensions.Run(jobData, lodGroupInstances.Length);
		}
		m_CrossfadedRendererCount += num2;
		lodGroupInstances.Dispose();
	}

	public void FreeLODGroupData(NativeArray<int> destroyedLODGroupsID)
	{
		if (destroyedLODGroupsID.Length != 0)
		{
			int num = LODGroupDataPoolBurst.FreeLODGroupData(in destroyedLODGroupsID, ref m_LODGroupData, ref m_LODGroupDataHash, ref m_FreeLODGroupDataHandles);
			m_CrossfadedRendererCount -= num;
		}
	}
}
