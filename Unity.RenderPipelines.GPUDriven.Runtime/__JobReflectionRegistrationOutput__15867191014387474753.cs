using System;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

[Unity.Jobs.DOTSCompilerGenerated]
internal class __JobReflectionRegistrationOutput__15867191014387474753
{
	public static void CreateJobReflectionData()
	{
		try
		{
			IJobParallelForBatchExtensions.EarlyJobInit<GPUResidentDrawer.FindRenderersFromMaterialOrMeshJob>();
			IJobParallelForExtensions.EarlyJobInit<AnimateCrossFadeJob>();
			IJobParallelForExtensions.EarlyJobInit<CullingJob>();
			IJobParallelForExtensions.EarlyJobInit<AllocateBinsPerBatch>();
			IJobExtensions.EarlyJobInit<PrefixSumDrawsAndInstances>();
			IJobParallelForExtensions.EarlyJobInit<DrawCommandOutputPerBatch>();
			IJobParallelForBatchExtensions.EarlyJobInit<CompactVisibilityMasksJob>();
			IJobExtensions.EarlyJobInit<PrefixSumDrawInstancesJob>();
			IJobParallelForExtensions.EarlyJobInit<BuildDrawListsJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<FindDrawInstancesJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<FindMaterialDrawInstancesJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<FindNonRegisteredMeshesJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<FindNonRegisteredMaterialsJob>();
			IJobParallelForExtensions.EarlyJobInit<RegisterNewMeshesJob>();
			IJobParallelForExtensions.EarlyJobInit<RegisterNewMaterialsJob>();
			IJobExtensions.EarlyJobInit<UpdatePackedMaterialDataCacheJob>();
			IJobParallelForExtensions.EarlyJobInit<GPUInstanceDataBuffer.ConvertCPUInstancesToGPUInstancesJob>();
			IJobParallelForExtensions.EarlyJobInit<GPUInstanceDataBufferUploader.WriteInstanceDataParameterJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<InstanceDataSystem.QueryRendererGroupInstancesCountJob>();
			IJobExtensions.EarlyJobInit<InstanceDataSystem.ComputeInstancesOffsetAndResizeInstancesArrayJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<InstanceDataSystem.QueryRendererGroupInstancesJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<InstanceDataSystem.QueryRendererGroupInstancesMultiJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<InstanceDataSystem.QuerySortedMeshInstancesJob>();
			IJobParallelForExtensions.EarlyJobInit<InstanceDataSystem.CalculateInterpolatedLightAndOcclusionProbesBatchJob>();
			IJobParallelForExtensions.EarlyJobInit<InstanceDataSystem.ScatterTetrahedronCacheIndicesJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<InstanceDataSystem.TransformUpdateJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<InstanceDataSystem.ProbesUpdateJob>();
			IJobParallelForExtensions.EarlyJobInit<InstanceDataSystem.MotionUpdateJob>();
			IJobParallelForExtensions.EarlyJobInit<InstanceDataSystem.UpdateRendererInstancesJob>();
			IJobParallelForExtensions.EarlyJobInit<InstanceDataSystem.CollectInstancesLODGroupsAndMasksJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<InstanceDataSystem.GetVisibleNonProcessedTreeInstancesJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<InstanceDataSystem.UpdateCompactedInstanceVisibilityJob>();
			IJobParallelForExtensions.EarlyJobInit<UpdateLODGroupTransformJob>();
			IJobParallelForExtensions.EarlyJobInit<UpdateLODGroupDataJob>();
			IJobForExtensions.EarlyJobInit<ParallelSortExtensions.RadixSortBucketCountJob>();
			IJobForExtensions.EarlyJobInit<ParallelSortExtensions.RadixSortBatchPrefixSumJob>();
			IJobForExtensions.EarlyJobInit<ParallelSortExtensions.RadixSortPrefixSumJob>();
			IJobForExtensions.EarlyJobInit<ParallelSortExtensions.RadixSortBucketSortJob>();
		}
		catch (Exception ex)
		{
			EarlyInitHelpers.JobReflectionDataCreationFailed(ex);
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	public static void EarlyInit()
	{
		CreateJobReflectionData();
	}
}
