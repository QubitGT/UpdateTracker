using System;
using Unity.Collections;
using Unity.Jobs;

namespace UnityEngine.Rendering;

internal class RenderersBatchersContext : IDisposable
{
	private InstanceDataSystem m_InstanceDataSystem;

	private GPUResidentDrawerResources m_Resources;

	private GPUDrivenProcessor m_GPUDrivenProcessor;

	private LODGroupDataPool m_LODGroupDataPool;

	internal GPUInstanceDataBuffer m_InstanceDataBuffer;

	private RenderersParameters m_RenderersParameters;

	private GPUInstanceDataBufferUploader.GPUResources m_UploadResources;

	private GPUInstanceDataBufferGrower.GPUResources m_GrowerResources;

	internal CommandBuffer m_CmdBuffer;

	private SphericalHarmonicsL2 m_CachedAmbientProbe;

	private float m_SmallMeshScreenPercentage;

	private GPUDrivenLODGroupDataCallback m_UpdateLODGroupCallback;

	private GPUDrivenLODGroupDataCallback m_TransformLODGroupCallback;

	private OcclusionCullingCommon m_OcclusionCullingCommon;

	private DebugRendererBatcherStats m_DebugStats;

	public RenderersParameters renderersParameters => m_RenderersParameters;

	public GraphicsBuffer gpuInstanceDataBuffer => m_InstanceDataBuffer.gpuBuffer;

	public int activeLodGroupCount => m_LODGroupDataPool.activeLodGroupCount;

	public NativeArray<GPUInstanceComponentDesc>.ReadOnly defaultDescriptions => m_InstanceDataBuffer.descriptions.AsReadOnly();

	public NativeArray<MetadataValue> defaultMetadata => m_InstanceDataBuffer.defaultMetadata;

	public NativeList<LODGroupCullingData> lodGroupCullingData => m_LODGroupDataPool.lodGroupCullingData;

	public int instanceDataBufferVersion => m_InstanceDataBuffer.version;

	public int instanceDataBufferLayoutVersion => m_InstanceDataBuffer.layoutVersion;

	public SphericalHarmonicsL2 cachedAmbientProbe => m_CachedAmbientProbe;

	public bool hasBoundingSpheres => m_InstanceDataSystem.hasBoundingSpheres;

	public int cameraCount => m_InstanceDataSystem.cameraCount;

	public CPUInstanceData.ReadOnly instanceData => m_InstanceDataSystem.instanceData;

	public CPUSharedInstanceData.ReadOnly sharedInstanceData => m_InstanceDataSystem.sharedInstanceData;

	public CPUPerCameraInstanceData perCameraInstanceData => m_InstanceDataSystem.perCameraInstanceData;

	public GPUInstanceDataBuffer.ReadOnly instanceDataBuffer => m_InstanceDataBuffer.AsReadOnly();

	public NativeArray<InstanceHandle> aliveInstances => m_InstanceDataSystem.aliveInstances;

	public float smallMeshScreenPercentage => m_SmallMeshScreenPercentage;

	public GPUResidentDrawerResources resources => m_Resources;

	internal OcclusionCullingCommon occlusionCullingCommon => m_OcclusionCullingCommon;

	internal DebugRendererBatcherStats debugStats => m_DebugStats;

	public RenderersBatchersContext(in RenderersBatchersContextDesc desc, GPUDrivenProcessor gpuDrivenProcessor, GPUResidentDrawerResources resources)
	{
		m_Resources = resources;
		m_GPUDrivenProcessor = gpuDrivenProcessor;
		RenderersParameters.Flags flags = RenderersParameters.Flags.None;
		if (desc.enableBoundingSpheresInstanceData)
		{
			flags |= RenderersParameters.Flags.UseBoundingSphereParameter;
		}
		m_InstanceDataBuffer = RenderersParameters.CreateInstanceDataBuffer(flags, in desc.instanceNumInfo);
		m_RenderersParameters = new RenderersParameters(in m_InstanceDataBuffer);
		m_LODGroupDataPool = new LODGroupDataPool(resources, desc.instanceNumInfo.GetInstanceNum(InstanceType.MeshRenderer), desc.supportDitheringCrossFade);
		m_UploadResources = default(GPUInstanceDataBufferUploader.GPUResources);
		m_UploadResources.LoadShaders(resources);
		m_GrowerResources = default(GPUInstanceDataBufferGrower.GPUResources);
		m_GrowerResources.LoadShaders(resources);
		m_CmdBuffer = new CommandBuffer();
		m_CmdBuffer.name = "GPUCullingCommands";
		m_CachedAmbientProbe = RenderSettings.ambientProbe;
		m_InstanceDataSystem = new InstanceDataSystem(desc.instanceNumInfo.GetTotalInstanceNum(), desc.enableBoundingSpheresInstanceData, resources);
		m_SmallMeshScreenPercentage = desc.smallMeshScreenPercentage;
		m_UpdateLODGroupCallback = UpdateLODGroupData;
		m_TransformLODGroupCallback = TransformLODGroupData;
		m_OcclusionCullingCommon = new OcclusionCullingCommon();
		m_OcclusionCullingCommon.Init(resources);
		m_DebugStats = (desc.enableCullerDebugStats ? new DebugRendererBatcherStats() : null);
	}

	public void Dispose()
	{
		NativeArray<int>.ReadOnly source = m_InstanceDataSystem.sharedInstanceData.rendererGroupIDs;
		if (source.Length > 0)
		{
			m_GPUDrivenProcessor.DisableGPUDrivenRendering(source);
		}
		m_InstanceDataSystem.Dispose();
		m_CmdBuffer.Release();
		m_GrowerResources.Dispose();
		m_UploadResources.Dispose();
		m_LODGroupDataPool.Dispose();
		m_InstanceDataBuffer.Dispose();
		m_UpdateLODGroupCallback = null;
		m_TransformLODGroupCallback = null;
		m_DebugStats?.Dispose();
		m_DebugStats = null;
		m_OcclusionCullingCommon?.Dispose();
		m_OcclusionCullingCommon = null;
	}

	public int GetMaxInstancesOfType(InstanceType instanceType)
	{
		return m_InstanceDataSystem.GetMaxInstancesOfType(instanceType);
	}

	public int GetAliveInstancesOfType(InstanceType instanceType)
	{
		return m_InstanceDataSystem.GetAliveInstancesOfType(instanceType);
	}

	public void GrowInstanceBuffer(in InstanceNumInfo instanceNumInfo)
	{
		using (GPUInstanceDataBufferGrower gPUInstanceDataBufferGrower = new GPUInstanceDataBufferGrower(m_InstanceDataBuffer, in instanceNumInfo))
		{
			GPUInstanceDataBuffer gPUInstanceDataBuffer = gPUInstanceDataBufferGrower.SubmitToGpu(ref m_GrowerResources);
			if (gPUInstanceDataBuffer != m_InstanceDataBuffer)
			{
				if (m_InstanceDataBuffer != null)
				{
					m_InstanceDataBuffer.Dispose();
				}
				m_InstanceDataBuffer = gPUInstanceDataBuffer;
			}
		}
		m_RenderersParameters = new RenderersParameters(in m_InstanceDataBuffer);
	}

	private void EnsureInstanceBufferCapacity()
	{
		int maxInstancesOfType = m_InstanceDataSystem.GetMaxInstancesOfType(InstanceType.MeshRenderer);
		int maxInstancesOfType2 = m_InstanceDataSystem.GetMaxInstancesOfType(InstanceType.SpeedTree);
		int num = m_InstanceDataBuffer.instanceNumInfo.GetInstanceNum(InstanceType.MeshRenderer);
		int num2 = m_InstanceDataBuffer.instanceNumInfo.GetInstanceNum(InstanceType.SpeedTree);
		bool flag = false;
		if (maxInstancesOfType > num)
		{
			flag = true;
			num = maxInstancesOfType + 1024;
		}
		if (maxInstancesOfType2 > num2)
		{
			flag = true;
			num2 = maxInstancesOfType2 + 256;
		}
		if (flag)
		{
			InstanceNumInfo instanceNumInfo = new InstanceNumInfo(num, num2);
			GrowInstanceBuffer(in instanceNumInfo);
		}
	}

	private void UpdateLODGroupData(in GPUDrivenLODGroupData lodGroupData)
	{
		m_LODGroupDataPool.UpdateLODGroupData(in lodGroupData);
	}

	private void TransformLODGroupData(in GPUDrivenLODGroupData lodGroupData)
	{
		m_LODGroupDataPool.UpdateLODGroupTransformData(in lodGroupData);
	}

	public void DestroyLODGroups(NativeArray<int> destroyed)
	{
		if (destroyed.Length != 0)
		{
			m_LODGroupDataPool.FreeLODGroupData(destroyed);
		}
	}

	public void UpdateLODGroups(NativeArray<int> changedID)
	{
		if (changedID.Length != 0)
		{
			m_GPUDrivenProcessor.DispatchLODGroupData(changedID, m_UpdateLODGroupCallback);
		}
	}

	public void ReallocateAndGetInstances(in GPUDrivenRendererGroupData rendererData, NativeArray<InstanceHandle> instances)
	{
		m_InstanceDataSystem.ReallocateAndGetInstances(in rendererData, instances);
		EnsureInstanceBufferCapacity();
	}

	public JobHandle ScheduleUpdateInstanceDataJob(NativeArray<InstanceHandle> instances, in GPUDrivenRendererGroupData rendererData)
	{
		return m_InstanceDataSystem.ScheduleUpdateInstanceDataJob(instances, in rendererData, m_LODGroupDataPool.lodGroupDataHash);
	}

	public void FreeRendererGroupInstances(NativeArray<int> rendererGroupsID)
	{
		m_InstanceDataSystem.FreeRendererGroupInstances(rendererGroupsID);
	}

	public void FreeInstances(NativeArray<InstanceHandle> instances)
	{
		m_InstanceDataSystem.FreeInstances(instances);
	}

	public JobHandle ScheduleQueryRendererGroupInstancesJob(NativeArray<int> rendererGroupIDs, NativeArray<InstanceHandle> instances)
	{
		return m_InstanceDataSystem.ScheduleQueryRendererGroupInstancesJob(rendererGroupIDs, instances);
	}

	public JobHandle ScheduleQueryRendererGroupInstancesJob(NativeArray<int> rendererGroupIDs, NativeList<InstanceHandle> instances)
	{
		return m_InstanceDataSystem.ScheduleQueryRendererGroupInstancesJob(rendererGroupIDs, instances);
	}

	public JobHandle ScheduleQueryRendererGroupInstancesJob(NativeArray<int> rendererGroupIDs, NativeArray<int> instancesOffset, NativeArray<int> instancesCount, NativeList<InstanceHandle> instances)
	{
		return m_InstanceDataSystem.ScheduleQueryRendererGroupInstancesJob(rendererGroupIDs, instancesOffset, instancesCount, instances);
	}

	public JobHandle ScheduleQueryMeshInstancesJob(NativeArray<int> sortedMeshIDs, NativeList<InstanceHandle> instances)
	{
		return m_InstanceDataSystem.ScheduleQuerySortedMeshInstancesJob(sortedMeshIDs, instances);
	}

	public void ChangeInstanceBufferVersion()
	{
		m_InstanceDataBuffer.version++;
	}

	public GPUInstanceDataBufferUploader CreateDataBufferUploader(int capacity, InstanceType instanceType)
	{
		return new GPUInstanceDataBufferUploader(in m_InstanceDataBuffer.descriptions, capacity, instanceType);
	}

	public void SubmitToGpu(NativeArray<InstanceHandle> instances, ref GPUInstanceDataBufferUploader uploader, bool submitOnlyWrittenParams)
	{
		uploader.SubmitToGpu(m_InstanceDataBuffer, instances, ref m_UploadResources, submitOnlyWrittenParams);
	}

	public void SubmitToGpu(NativeArray<GPUInstanceIndex> gpuInstanceIndices, ref GPUInstanceDataBufferUploader uploader, bool submitOnlyWrittenParams)
	{
		uploader.SubmitToGpu(m_InstanceDataBuffer, gpuInstanceIndices, ref m_UploadResources, submitOnlyWrittenParams);
	}

	public void InitializeInstanceTransforms(NativeArray<InstanceHandle> instances, NativeArray<Matrix4x4> localToWorldMatrices, NativeArray<Matrix4x4> prevLocalToWorldMatrices)
	{
		if (instances.Length != 0)
		{
			m_InstanceDataSystem.InitializeInstanceTransforms(instances, localToWorldMatrices, prevLocalToWorldMatrices, in m_RenderersParameters, m_InstanceDataBuffer);
			ChangeInstanceBufferVersion();
		}
	}

	public void UpdateInstanceTransforms(NativeArray<InstanceHandle> instances, NativeArray<Matrix4x4> localToWorldMatrices)
	{
		if (instances.Length != 0)
		{
			m_InstanceDataSystem.UpdateInstanceTransforms(instances, localToWorldMatrices, in m_RenderersParameters, m_InstanceDataBuffer);
			ChangeInstanceBufferVersion();
		}
	}

	public void UpdateAmbientProbeAndGpuBuffer(bool forceUpdate)
	{
		if (forceUpdate || m_CachedAmbientProbe != RenderSettings.ambientProbe)
		{
			m_CachedAmbientProbe = RenderSettings.ambientProbe;
			m_InstanceDataSystem.UpdateAllInstanceProbes(in m_RenderersParameters, m_InstanceDataBuffer);
			ChangeInstanceBufferVersion();
		}
	}

	public void UpdateInstanceWindDataHistory(NativeArray<GPUInstanceIndex> gpuInstanceIndices)
	{
		if (gpuInstanceIndices.Length != 0)
		{
			m_InstanceDataSystem.UpdateInstanceWindDataHistory(gpuInstanceIndices, m_RenderersParameters, m_InstanceDataBuffer);
			ChangeInstanceBufferVersion();
		}
	}

	public void UpdateInstanceMotions()
	{
		m_InstanceDataSystem.UpdateInstanceMotions(in m_RenderersParameters, m_InstanceDataBuffer);
		ChangeInstanceBufferVersion();
	}

	public void TransformLODGroups(NativeArray<int> lodGroupsID)
	{
		if (lodGroupsID.Length != 0)
		{
			m_GPUDrivenProcessor.DispatchLODGroupData(lodGroupsID, m_TransformLODGroupCallback);
		}
	}

	public void UpdatePerFrameInstanceVisibility(in ParallelBitArray compactedVisibilityMasks)
	{
		m_InstanceDataSystem.UpdatePerFrameInstanceVisibility(in compactedVisibilityMasks);
	}

	public JobHandle ScheduleCollectInstancesLODGroupAndMasksJob(NativeArray<InstanceHandle> instances, NativeArray<uint> lodGroupAndMasks)
	{
		return m_InstanceDataSystem.ScheduleCollectInstancesLODGroupAndMasksJob(instances, lodGroupAndMasks);
	}

	public InstanceHandle GetRendererInstanceHandle(int rendererID)
	{
		NativeArray<int> rendererGroupIDs = new NativeArray<int>(1, Allocator.TempJob);
		NativeArray<InstanceHandle> instances = new NativeArray<InstanceHandle>(1, Allocator.TempJob);
		rendererGroupIDs[0] = rendererID;
		m_InstanceDataSystem.ScheduleQueryRendererGroupInstancesJob(rendererGroupIDs, instances).Complete();
		InstanceHandle result = instances[0];
		rendererGroupIDs.Dispose();
		instances.Dispose();
		return result;
	}

	public void GetVisibleTreeInstances(in ParallelBitArray compactedVisibilityMasks, in ParallelBitArray processedBits, NativeList<int> visibeTreeRendererIDs, NativeList<InstanceHandle> visibeTreeInstances, bool becomeVisibleOnly, out int becomeVisibeTreeInstancesCount)
	{
		m_InstanceDataSystem.GetVisibleTreeInstances(in compactedVisibilityMasks, in processedBits, visibeTreeRendererIDs, visibeTreeInstances, becomeVisibleOnly, out becomeVisibeTreeInstancesCount);
	}

	public GPUInstanceDataBuffer GetInstanceDataBuffer()
	{
		return m_InstanceDataBuffer;
	}

	public void UpdateFrame()
	{
		m_OcclusionCullingCommon.UpdateFrame();
		if (m_DebugStats != null)
		{
			m_OcclusionCullingCommon.UpdateOccluderStats(m_DebugStats);
		}
	}

	public void FreePerCameraInstanceData(NativeArray<int> cameraIDs)
	{
		m_InstanceDataSystem.DeallocatePerCameraInstanceData(cameraIDs);
	}

	public void UpdateCameras(NativeArray<int> cameraIDs)
	{
		m_InstanceDataSystem.AllocatePerCameraInstanceData(cameraIDs);
	}
}
