using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UnityEngine.Rendering;

internal class InstanceCullingBatcher : IDisposable
{
	private RenderersBatchersContext m_BatchersContext;

	private CPUDrawInstanceData m_DrawInstanceData;

	private BatchRendererGroup m_BRG;

	private NativeParallelHashMap<uint, BatchID> m_GlobalBatchIDs;

	private InstanceCuller m_Culler;

	private NativeParallelHashMap<int, BatchMaterialID> m_BatchMaterialHash;

	private NativeParallelHashMap<int, GPUDrivenPackedMaterialData> m_PackedMaterialHash;

	private NativeParallelHashMap<int, BatchMeshID> m_BatchMeshHash;

	private int m_CachedInstanceDataBufferLayoutVersion;

	private OnCullingCompleteCallback m_OnCompleteCallback;

	public NativeParallelHashMap<int, BatchMaterialID> batchMaterialHash => m_BatchMaterialHash;

	public NativeParallelHashMap<int, GPUDrivenPackedMaterialData> packedMaterialHash => m_PackedMaterialHash;

	internal ref InstanceCuller culler => ref m_Culler;

	public InstanceCullingBatcher(RenderersBatchersContext batcherContext, InstanceCullingBatcherDesc desc, BatchRendererGroup.OnFinishedCulling onFinishedCulling)
	{
		m_BatchersContext = batcherContext;
		m_DrawInstanceData = new CPUDrawInstanceData();
		m_DrawInstanceData.Initialize();
		m_BRG = new BatchRendererGroup(new BatchRendererGroupCreateInfo
		{
			cullingCallback = OnPerformCulling,
			finishedCullingCallback = onFinishedCulling,
			userContext = IntPtr.Zero
		});
		m_Culler = default(InstanceCuller);
		m_Culler.Init(batcherContext.resources, batcherContext.debugStats);
		m_CachedInstanceDataBufferLayoutVersion = -1;
		m_OnCompleteCallback = desc.onCompleteCallback;
		m_BatchMaterialHash = new NativeParallelHashMap<int, BatchMaterialID>(64, Allocator.Persistent);
		m_PackedMaterialHash = new NativeParallelHashMap<int, GPUDrivenPackedMaterialData>(64, Allocator.Persistent);
		m_BatchMeshHash = new NativeParallelHashMap<int, BatchMeshID>(64, Allocator.Persistent);
		m_GlobalBatchIDs = new NativeParallelHashMap<uint, BatchID>(6, Allocator.Persistent);
		m_GlobalBatchIDs.Add(1u, GetBatchID(InstanceComponentGroup.Default));
		m_GlobalBatchIDs.Add(3u, GetBatchID(InstanceComponentGroup.DefaultWind));
		m_GlobalBatchIDs.Add(5u, GetBatchID(InstanceComponentGroup.DefaultLightProbe));
		m_GlobalBatchIDs.Add(9u, GetBatchID(InstanceComponentGroup.DefaultLightmap));
		m_GlobalBatchIDs.Add(7u, GetBatchID(InstanceComponentGroup.DefaultWindLightProbe));
		m_GlobalBatchIDs.Add(11u, GetBatchID(InstanceComponentGroup.DefaultWindLightmap));
	}

	public void Dispose()
	{
		m_OnCompleteCallback = null;
		m_Culler.Dispose();
		foreach (KeyValue<uint, BatchID> globalBatchID in m_GlobalBatchIDs)
		{
			if (!globalBatchID.Value.Equals(BatchID.Null))
			{
				m_BRG.RemoveBatch(globalBatchID.Value);
			}
		}
		m_GlobalBatchIDs.Dispose();
		if (m_BRG != null)
		{
			m_BRG.Dispose();
		}
		m_DrawInstanceData.Dispose();
		m_DrawInstanceData = null;
		m_BatchMaterialHash.Dispose();
		m_PackedMaterialHash.Dispose();
		m_BatchMeshHash.Dispose();
	}

	private BatchID GetBatchID(InstanceComponentGroup componentsOverriden)
	{
		if (m_CachedInstanceDataBufferLayoutVersion != m_BatchersContext.instanceDataBufferLayoutVersion)
		{
			return BatchID.Null;
		}
		NativeList<MetadataValue> nativeList = new NativeList<MetadataValue>(m_BatchersContext.defaultMetadata.Length, Allocator.Temp);
		for (int i = 0; i < m_BatchersContext.defaultDescriptions.Length; i++)
		{
			InstanceComponentGroup componentGroup = m_BatchersContext.defaultDescriptions[i].componentGroup;
			MetadataValue metadataValue = m_BatchersContext.defaultMetadata[i];
			uint num = metadataValue.Value;
			if ((componentsOverriden & componentGroup) == 0)
			{
				num &= 0x4FFFFFFFu;
			}
			MetadataValue value = new MetadataValue
			{
				NameID = metadataValue.NameID,
				Value = num
			};
			nativeList.Add(in value);
		}
		return m_BRG.AddBatch(nativeList.AsArray(), m_BatchersContext.gpuInstanceDataBuffer.bufferHandle);
	}

	private void UpdateInstanceDataBufferLayoutVersion()
	{
		if (m_CachedInstanceDataBufferLayoutVersion == m_BatchersContext.instanceDataBufferLayoutVersion)
		{
			return;
		}
		m_CachedInstanceDataBufferLayoutVersion = m_BatchersContext.instanceDataBufferLayoutVersion;
		foreach (KeyValue<uint, BatchID> globalBatchID in m_GlobalBatchIDs)
		{
			BatchID value = globalBatchID.Value;
			if (!value.Equals(BatchID.Null))
			{
				m_BRG.RemoveBatch(value);
			}
			InstanceComponentGroup key = (InstanceComponentGroup)globalBatchID.Key;
			globalBatchID.Value = GetBatchID(key);
		}
	}

	public CPUDrawInstanceData GetDrawInstanceData()
	{
		return m_DrawInstanceData;
	}

	public JobHandle OnPerformCulling(BatchRendererGroup rendererGroup, BatchCullingContext cc, BatchCullingOutput cullingOutput, IntPtr userContext)
	{
		foreach (KeyValue<uint, BatchID> globalBatchID in m_GlobalBatchIDs)
		{
			if (globalBatchID.Value.Equals(BatchID.Null))
			{
				return default(JobHandle);
			}
		}
		m_DrawInstanceData.RebuildDrawListsIfNeeded();
		bool hasBoundingSpheres = m_BatchersContext.hasBoundingSpheres;
		ref InstanceCuller reference = ref m_Culler;
		BatchCullingOutput cullingOutput2 = cullingOutput;
		CPUInstanceData.ReadOnly instanceData = m_BatchersContext.instanceData;
		CPUSharedInstanceData.ReadOnly sharedInstanceData = m_BatchersContext.sharedInstanceData;
		CPUPerCameraInstanceData perCameraInstanceData = m_BatchersContext.perCameraInstanceData;
		GPUInstanceDataBuffer.ReadOnly instanceDataBuffer = m_BatchersContext.instanceDataBuffer;
		JobHandle jobHandle = reference.CreateCullJobTree(in cc, cullingOutput2, in instanceData, in sharedInstanceData, in perCameraInstanceData, in instanceDataBuffer, m_BatchersContext.lodGroupCullingData, m_DrawInstanceData, m_GlobalBatchIDs, m_BatchersContext.smallMeshScreenPercentage, hasBoundingSpheres ? m_BatchersContext.occlusionCullingCommon : null);
		if (m_OnCompleteCallback != null)
		{
			m_OnCompleteCallback(jobHandle, in cc, in cullingOutput);
		}
		return jobHandle;
	}

	public void OnFinishedCulling(IntPtr customCullingResult)
	{
		int viewInstanceID = (int)customCullingResult;
		m_Culler.EnsureValidOcclusionTestResults(viewInstanceID);
	}

	public void DestroyDrawInstances(NativeArray<InstanceHandle> instances)
	{
		if (instances.Length != 0)
		{
			m_DrawInstanceData.DestroyDrawInstances(instances);
		}
	}

	public void DestroyMaterials(NativeArray<int> destroyedMaterials)
	{
		if (destroyedMaterials.Length == 0)
		{
			return;
		}
		NativeList<uint> nativeList = new NativeList<uint>(destroyedMaterials.Length, Allocator.TempJob);
		foreach (int item2 in destroyedMaterials)
		{
			if (m_BatchMaterialHash.TryGetValue(item2, out var item))
			{
				nativeList.Add(in item.value);
				m_BatchMaterialHash.Remove(item2);
				m_PackedMaterialHash.Remove(item2);
				m_BRG.UnregisterMaterial(item);
			}
		}
		m_DrawInstanceData.DestroyMaterialDrawInstances(nativeList.AsArray());
		nativeList.Dispose();
	}

	public void DestroyMeshes(NativeArray<int> destroyedMeshes)
	{
		if (destroyedMeshes.Length == 0)
		{
			return;
		}
		foreach (int item2 in destroyedMeshes)
		{
			if (m_BatchMeshHash.TryGetValue(item2, out var item))
			{
				m_BatchMeshHash.Remove(item2);
				m_BRG.UnregisterMesh(item);
			}
		}
	}

	public void PostCullBeginCameraRendering(RenderRequestBatcherContext context)
	{
	}

	private void RegisterBatchMeshes(NativeArray<int> meshIDs)
	{
		NativeList<int> nativeList = new NativeList<int>(meshIDs.Length, Allocator.TempJob);
		FindNonRegisteredMeshesJob jobData = default(FindNonRegisteredMeshesJob);
		jobData.instanceIDs = meshIDs;
		jobData.hashMap = m_BatchMeshHash;
		jobData.outInstancesWriter = nativeList.AsParallelWriter();
		jobData.ScheduleBatch(meshIDs.Length, 128).Complete();
		NativeArray<BatchMeshID> source = new NativeArray<BatchMeshID>(nativeList.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		BatchRendererGroup bRG = m_BRG;
		NativeArray<int> source2 = nativeList.AsArray();
		bRG.RegisterMeshes(source2, source);
		int num = m_BatchMeshHash.Count() + source.Length;
		m_BatchMeshHash.Capacity = Math.Max(m_BatchMeshHash.Capacity, Mathf.CeilToInt((float)num / 1023f) * 1024);
		RegisterNewMeshesJob jobData2 = default(RegisterNewMeshesJob);
		jobData2.instanceIDs = nativeList.AsArray();
		jobData2.batchIDs = source;
		jobData2.hashMap = m_BatchMeshHash.AsParallelWriter();
		IJobParallelForExtensions.Schedule(jobData2, nativeList.Length, 128).Complete();
		nativeList.Dispose();
		source.Dispose();
	}

	private void RegisterBatchMaterials(in NativeArray<int> usedMaterialIDs, in NativeArray<GPUDrivenPackedMaterialData> usedPackedMaterialDatas)
	{
		NativeList<int> nativeList = new NativeList<int>(usedMaterialIDs.Length, Allocator.TempJob);
		NativeList<GPUDrivenPackedMaterialData> nativeList2 = new NativeList<GPUDrivenPackedMaterialData>(usedMaterialIDs.Length, Allocator.TempJob);
		FindNonRegisteredMaterialsJob jobData = default(FindNonRegisteredMaterialsJob);
		jobData.instanceIDs = usedMaterialIDs;
		jobData.packedMaterialDatas = usedPackedMaterialDatas;
		jobData.hashMap = m_BatchMaterialHash;
		jobData.outInstancesWriter = nativeList.AsParallelWriter();
		jobData.outPackedMaterialDatasWriter = nativeList2.AsParallelWriter();
		jobData.ScheduleBatch(usedMaterialIDs.Length, 128).Complete();
		NativeArray<BatchMaterialID> source = new NativeArray<BatchMaterialID>(nativeList.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		BatchRendererGroup bRG = m_BRG;
		NativeArray<int> source2 = nativeList.AsArray();
		bRG.RegisterMaterials(source2, source);
		int num = m_BatchMaterialHash.Count() + nativeList.Length;
		m_BatchMaterialHash.Capacity = Math.Max(m_BatchMaterialHash.Capacity, Mathf.CeilToInt((float)num / 1023f) * 1024);
		m_PackedMaterialHash.Capacity = m_BatchMaterialHash.Capacity;
		RegisterNewMaterialsJob jobData2 = default(RegisterNewMaterialsJob);
		jobData2.instanceIDs = nativeList.AsArray();
		jobData2.packedMaterialDatas = nativeList2.AsArray();
		jobData2.batchIDs = source;
		jobData2.batchMaterialHashMap = m_BatchMaterialHash.AsParallelWriter();
		jobData2.packedMaterialHashMap = m_PackedMaterialHash.AsParallelWriter();
		IJobParallelForExtensions.Schedule(jobData2, nativeList.Length, 128).Complete();
		nativeList.Dispose();
		nativeList2.Dispose();
		source.Dispose();
	}

	public JobHandle SchedulePackedMaterialCacheUpdate(NativeArray<int> materialIDs, NativeArray<GPUDrivenPackedMaterialData> packedMaterialDatas)
	{
		UpdatePackedMaterialDataCacheJob jobData = default(UpdatePackedMaterialDataCacheJob);
		jobData.materialIDs = materialIDs.AsReadOnly();
		jobData.packedMaterialDatas = packedMaterialDatas.AsReadOnly();
		jobData.packedMaterialHash = m_PackedMaterialHash;
		return jobData.Schedule();
	}

	public void BuildBatch(NativeArray<InstanceHandle> instances, in GPUDrivenRendererGroupData rendererData, bool registerMaterialsAndMeshes)
	{
		if (registerMaterialsAndMeshes)
		{
			RegisterBatchMaterials(in rendererData.materialID, in rendererData.packedMaterialData);
			RegisterBatchMeshes(rendererData.meshID);
		}
		NativeParallelHashMap<RangeKey, int> rangeHash = m_DrawInstanceData.rangeHash;
		NativeList<DrawRange> drawRanges = m_DrawInstanceData.drawRanges;
		NativeParallelHashMap<DrawKey, int> batchHash = m_DrawInstanceData.batchHash;
		NativeList<DrawBatch> drawBatches = m_DrawInstanceData.drawBatches;
		NativeList<DrawInstance> drawInstances = m_DrawInstanceData.drawInstances;
		InstanceCullingBatcherBurst.CreateDrawBatches(rendererData.instancesCount.Length == 0, in instances, in rendererData, in m_BatchMeshHash, in m_BatchMaterialHash, in m_PackedMaterialHash, ref rangeHash, ref drawRanges, ref batchHash, ref drawBatches, ref drawInstances);
		m_DrawInstanceData.NeedsRebuild();
		UpdateInstanceDataBufferLayoutVersion();
	}

	public void InstanceOccludersUpdated(int viewInstanceID, int subviewMask)
	{
		m_Culler.InstanceOccludersUpdated(viewInstanceID, subviewMask, m_BatchersContext);
	}

	public void UpdateFrame()
	{
		m_Culler.UpdateFrame(m_BatchersContext.cameraCount);
	}

	public ParallelBitArray GetCompactedVisibilityMasks(bool syncCullingJobs)
	{
		return m_Culler.GetCompactedVisibilityMasks(syncCullingJobs);
	}

	public void OnEndContextRendering()
	{
		ParallelBitArray compactedVisibilityMasks = GetCompactedVisibilityMasks(syncCullingJobs: true);
		if (compactedVisibilityMasks.IsCreated)
		{
			m_BatchersContext.UpdatePerFrameInstanceVisibility(in compactedVisibilityMasks);
		}
	}

	public void OnBeginCameraRendering(Camera camera)
	{
		m_Culler.OnBeginCameraRendering(camera);
	}

	public void OnEndCameraRendering(Camera camera)
	{
		m_Culler.OnEndCameraRendering(camera);
	}
}
