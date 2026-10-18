using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering;

internal class GPUResidentBatcher : IDisposable
{
	private RenderersBatchersContext m_BatchersContext;

	private GPUDrivenProcessor m_GPUDrivenProcessor;

	private GPUDrivenRendererDataCallback m_UpdateRendererInstancesAndBatchesCallback;

	private GPUDrivenRendererDataCallback m_UpdateRendererBatchesCallback;

	private InstanceCullingBatcher m_InstanceCullingBatcher;

	private ParallelBitArray m_ProcessedThisFrameTreeBits;

	internal RenderersBatchersContext batchersContext => m_BatchersContext;

	internal OcclusionCullingCommon occlusionCullingCommon => m_BatchersContext.occlusionCullingCommon;

	internal InstanceCullingBatcher instanceCullingBatcher => m_InstanceCullingBatcher;

	public GPUResidentBatcher(RenderersBatchersContext batcherContext, InstanceCullingBatcherDesc instanceCullerBatcherDesc, GPUDrivenProcessor gpuDrivenProcessor)
	{
		m_BatchersContext = batcherContext;
		m_GPUDrivenProcessor = gpuDrivenProcessor;
		m_UpdateRendererInstancesAndBatchesCallback = UpdateRendererInstancesAndBatches;
		m_UpdateRendererBatchesCallback = UpdateRendererBatches;
		m_InstanceCullingBatcher = new InstanceCullingBatcher(batcherContext, instanceCullerBatcherDesc, OnFinishedCulling);
	}

	public void Dispose()
	{
		m_GPUDrivenProcessor.ClearMaterialFilters();
		m_InstanceCullingBatcher.Dispose();
		if (m_ProcessedThisFrameTreeBits.IsCreated)
		{
			m_ProcessedThisFrameTreeBits.Dispose();
		}
	}

	public void OnBeginContextRendering()
	{
		if (m_ProcessedThisFrameTreeBits.IsCreated)
		{
			m_ProcessedThisFrameTreeBits.Dispose();
		}
	}

	public void OnEndContextRendering()
	{
		m_InstanceCullingBatcher?.OnEndContextRendering();
	}

	public void OnBeginCameraRendering(Camera camera)
	{
		m_InstanceCullingBatcher?.OnBeginCameraRendering(camera);
	}

	public void OnEndCameraRendering(Camera camera)
	{
		m_InstanceCullingBatcher?.OnEndCameraRendering(camera);
	}

	public void UpdateFrame()
	{
		m_InstanceCullingBatcher.UpdateFrame();
		m_BatchersContext.UpdateFrame();
	}

	public void DestroyMaterials(NativeArray<int> destroyedMaterials)
	{
		m_InstanceCullingBatcher.DestroyMaterials(destroyedMaterials);
	}

	public void DestroyDrawInstances(NativeArray<InstanceHandle> instances)
	{
		m_InstanceCullingBatcher.DestroyDrawInstances(instances);
	}

	public void DestroyMeshes(NativeArray<int> destroyedMeshes)
	{
		m_InstanceCullingBatcher.DestroyMeshes(destroyedMeshes);
	}

	internal void FreeRendererGroupInstances(NativeArray<int> rendererGroupIDs)
	{
		if (rendererGroupIDs.Length != 0)
		{
			NativeList<InstanceHandle> instances = new NativeList<InstanceHandle>(rendererGroupIDs.Length, Allocator.TempJob);
			m_BatchersContext.ScheduleQueryRendererGroupInstancesJob(rendererGroupIDs, instances).Complete();
			DestroyDrawInstances(instances.AsArray());
			instances.Dispose();
			m_BatchersContext.FreeRendererGroupInstances(rendererGroupIDs);
		}
	}

	public void InstanceOcclusionTest(RenderGraph renderGraph, in OcclusionCullingSettings settings, ReadOnlySpan<SubviewOcclusionTest> subviewOcclusionTests)
	{
		if (m_BatchersContext.hasBoundingSpheres)
		{
			m_InstanceCullingBatcher.culler.InstanceOcclusionTest(renderGraph, in settings, subviewOcclusionTests, m_BatchersContext);
		}
	}

	public void UpdateInstanceOccluders(RenderGraph renderGraph, in OccluderParameters occluderParams, ReadOnlySpan<OccluderSubviewUpdate> occluderSubviewUpdates)
	{
		if (m_BatchersContext.hasBoundingSpheres)
		{
			m_BatchersContext.occlusionCullingCommon.UpdateInstanceOccluders(renderGraph, in occluderParams, occluderSubviewUpdates);
		}
	}

	public void UpdateRenderers(NativeArray<int> renderersID, bool materialUpdateOnly = false)
	{
		if (renderersID.Length != 0)
		{
			m_GPUDrivenProcessor.enablePartialRendering = false;
			m_GPUDrivenProcessor.EnableGPUDrivenRenderingAndDispatchRendererData(renderersID, materialUpdateOnly ? m_UpdateRendererBatchesCallback : m_UpdateRendererInstancesAndBatchesCallback, materialUpdateOnly);
			m_GPUDrivenProcessor.enablePartialRendering = false;
		}
	}

	public JobHandle SchedulePackedMaterialCacheUpdate(NativeArray<int> materialIDs, NativeArray<GPUDrivenPackedMaterialData> packedMaterialDatas)
	{
		return m_InstanceCullingBatcher.SchedulePackedMaterialCacheUpdate(materialIDs, packedMaterialDatas);
	}

	public void PostCullBeginCameraRendering(RenderRequestBatcherContext context)
	{
		m_InstanceCullingBatcher.PostCullBeginCameraRendering(context);
	}

	public void OnSetupAmbientProbe()
	{
		m_BatchersContext.UpdateAmbientProbeAndGpuBuffer(forceUpdate: false);
	}

	private void UpdateRendererInstancesAndBatches(in GPUDrivenRendererGroupData rendererData, IList<Mesh> meshes, IList<Material> materials)
	{
		FreeRendererGroupInstances(rendererData.invalidRendererGroupID);
		if (rendererData.rendererGroupID.Length != 0)
		{
			NativeArray<InstanceHandle> instances = new NativeArray<InstanceHandle>(rendererData.localToWorldMatrix.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			m_BatchersContext.ReallocateAndGetInstances(in rendererData, instances);
			JobHandle jobHandle = m_BatchersContext.ScheduleUpdateInstanceDataJob(instances, in rendererData);
			GPUInstanceDataBufferUploader uploader = m_BatchersContext.CreateDataBufferUploader(instances.Length, InstanceType.MeshRenderer);
			uploader.AllocateUploadHandles(instances.Length);
			JobHandle jobHandle2 = default(JobHandle);
			uploader.WriteInstanceDataJob(m_BatchersContext.renderersParameters.lightmapScale.index, rendererData.lightmapScaleOffset, rendererData.rendererGroupIndex).Complete();
			m_BatchersContext.SubmitToGpu(instances, ref uploader, submitOnlyWrittenParams: true);
			m_BatchersContext.ChangeInstanceBufferVersion();
			uploader.Dispose();
			jobHandle.Complete();
			m_BatchersContext.InitializeInstanceTransforms(instances, rendererData.localToWorldMatrix, rendererData.prevLocalToWorldMatrix);
			m_InstanceCullingBatcher.BuildBatch(instances, in rendererData, registerMaterialsAndMeshes: true);
			instances.Dispose();
		}
	}

	private void UpdateRendererBatches(in GPUDrivenRendererGroupData rendererData, IList<Mesh> meshes, IList<Material> materials)
	{
		if (rendererData.rendererGroupID.Length != 0)
		{
			NativeList<InstanceHandle> instances = new NativeList<InstanceHandle>(rendererData.localToWorldMatrix.Length, Allocator.TempJob);
			m_BatchersContext.ScheduleQueryRendererGroupInstancesJob(rendererData.rendererGroupID, instances).Complete();
			m_InstanceCullingBatcher.BuildBatch(instances.AsArray(), in rendererData, registerMaterialsAndMeshes: false);
			instances.Dispose();
		}
	}

	private void OnFinishedCulling(IntPtr customCullingResult)
	{
		ProcessTrees();
		m_InstanceCullingBatcher.OnFinishedCulling(customCullingResult);
	}

	private void ProcessTrees()
	{
		if (m_BatchersContext.GetAliveInstancesOfType(InstanceType.SpeedTree) == 0)
		{
			return;
		}
		ParallelBitArray compactedVisibilityMasks = m_InstanceCullingBatcher.GetCompactedVisibilityMasks(syncCullingJobs: false);
		if (!compactedVisibilityMasks.IsCreated)
		{
			return;
		}
		int length = m_BatchersContext.aliveInstances.Length;
		if (!m_ProcessedThisFrameTreeBits.IsCreated)
		{
			m_ProcessedThisFrameTreeBits = new ParallelBitArray(length, Allocator.TempJob);
		}
		else if (m_ProcessedThisFrameTreeBits.Length < length)
		{
			m_ProcessedThisFrameTreeBits.Resize(length);
		}
		bool becomeVisibleOnly = !Application.isPlaying;
		NativeList<int> visibeTreeRendererIDs = new NativeList<int>(Allocator.TempJob);
		NativeList<InstanceHandle> visibeTreeInstances = new NativeList<InstanceHandle>(Allocator.TempJob);
		m_BatchersContext.GetVisibleTreeInstances(in compactedVisibilityMasks, in m_ProcessedThisFrameTreeBits, visibeTreeRendererIDs, visibeTreeInstances, becomeVisibleOnly, out var becomeVisibeTreeInstancesCount);
		if (visibeTreeRendererIDs.Length > 0)
		{
			NativeArray<int> subArray = visibeTreeRendererIDs.AsArray().GetSubArray(0, becomeVisibeTreeInstancesCount);
			NativeArray<InstanceHandle> subArray2 = visibeTreeInstances.AsArray().GetSubArray(0, becomeVisibeTreeInstancesCount);
			if (subArray.Length > 0)
			{
				UpdateSpeedTreeWindAndUploadWindParamsToGPU(subArray, subArray2, history: true);
			}
			UpdateSpeedTreeWindAndUploadWindParamsToGPU(visibeTreeRendererIDs.AsArray(), visibeTreeInstances.AsArray(), history: false);
		}
		visibeTreeRendererIDs.Dispose();
		visibeTreeInstances.Dispose();
	}

	private unsafe void UpdateSpeedTreeWindAndUploadWindParamsToGPU(NativeArray<int> treeRendererIDs, NativeArray<InstanceHandle> treeInstances, bool history)
	{
		if (treeRendererIDs.Length != 0)
		{
			NativeArray<GPUInstanceIndex> gpuInstanceIndices = new NativeArray<GPUInstanceIndex>(treeInstances.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			m_BatchersContext.instanceDataBuffer.CPUInstanceArrayToGPUInstanceArray(treeInstances, gpuInstanceIndices);
			if (!history)
			{
				m_BatchersContext.UpdateInstanceWindDataHistory(gpuInstanceIndices);
			}
			GPUInstanceDataBufferUploader uploader = m_BatchersContext.CreateDataBufferUploader(treeInstances.Length, InstanceType.SpeedTree);
			uploader.AllocateUploadHandles(treeInstances.Length);
			SpeedTreeWindParamsBufferIterator windParams = default(SpeedTreeWindParamsBufferIterator);
			windParams.bufferPtr = uploader.GetUploadBufferPtr();
			for (int i = 0; i < 16; i++)
			{
				windParams.uintParamOffsets[i] = uploader.PrepareParamWrite<Vector4>(m_BatchersContext.renderersParameters.windParams[i].index);
			}
			windParams.uintStride = uploader.GetUIntPerInstance();
			windParams.elementOffset = 0;
			windParams.elementsCount = treeInstances.Length;
			SpeedTreeWindManager.UpdateWindAndWriteBufferWindParams(treeRendererIDs, windParams, history);
			m_BatchersContext.SubmitToGpu(gpuInstanceIndices, ref uploader, submitOnlyWrittenParams: true);
			gpuInstanceIndices.Dispose();
			uploader.Dispose();
		}
	}
}
