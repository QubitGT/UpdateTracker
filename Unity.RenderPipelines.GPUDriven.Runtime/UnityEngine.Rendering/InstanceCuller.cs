using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering;

internal struct InstanceCuller : IDisposable
{
	private struct AnimatedFadeData
	{
		public int cameraID;

		public JobHandle jobHandle;
	}

	private static class ShaderIDs
	{
		public static readonly int InstanceOcclusionCullerShaderVariables = Shader.PropertyToID("InstanceOcclusionCullerShaderVariables");

		public static readonly int _DrawInfo = Shader.PropertyToID("_DrawInfo");

		public static readonly int _InstanceInfo = Shader.PropertyToID("_InstanceInfo");

		public static readonly int _DrawArgs = Shader.PropertyToID("_DrawArgs");

		public static readonly int _InstanceIndices = Shader.PropertyToID("_InstanceIndices");

		public static readonly int _InstanceDataBuffer = Shader.PropertyToID("_InstanceDataBuffer");

		public static readonly int _OccluderDepthPyramid = Shader.PropertyToID("_OccluderDepthPyramid");

		public static readonly int _OcclusionDebugCounters = Shader.PropertyToID("_OcclusionDebugCounters");
	}

	private class InstanceOcclusionTestPassData
	{
		public OcclusionCullingSettings settings;

		public InstanceOcclusionTestSubviewSettings subviewSettings;

		public OccluderHandles occluderHandles;

		public IndirectBufferContextHandles bufferHandles;
	}

	private NativeParallelHashMap<int, AnimatedFadeData> m_LODParamsToCameraID;

	private ParallelBitArray m_CompactedVisibilityMasks;

	private JobHandle m_CompactedVisibilityMasksJobsHandle;

	private IndirectBufferContextStorage m_IndirectStorage;

	private OcclusionTestComputeShader m_OcclusionTestShader;

	private int m_ResetDrawArgsKernel;

	private int m_CopyInstancesKernel;

	private int m_CullInstancesKernel;

	private DebugRendererBatcherStats m_DebugStats;

	private InstanceCullerSplitDebugArray m_SplitDebugArray;

	private InstanceOcclusionEventDebugArray m_OcclusionEventDebugArray;

	private ProfilingSampler m_ProfilingSampleInstanceOcclusionTest;

	private NativeArray<InstanceOcclusionCullerShaderVariables> m_ShaderVariables;

	private ComputeBuffer m_ConstantBuffer;

	private CommandBuffer m_CommandBuffer;

	internal void Init(GPUResidentDrawerResources resources, DebugRendererBatcherStats debugStats = null)
	{
		m_IndirectStorage.Init();
		m_OcclusionTestShader.Init(resources.instanceOcclusionCullingKernels);
		m_ResetDrawArgsKernel = m_OcclusionTestShader.cs.FindKernel("ResetDrawArgs");
		m_CopyInstancesKernel = m_OcclusionTestShader.cs.FindKernel("CopyInstances");
		m_CullInstancesKernel = m_OcclusionTestShader.cs.FindKernel("CullInstances");
		m_DebugStats = debugStats;
		m_SplitDebugArray = default(InstanceCullerSplitDebugArray);
		m_SplitDebugArray.Init();
		m_OcclusionEventDebugArray = default(InstanceOcclusionEventDebugArray);
		m_OcclusionEventDebugArray.Init();
		m_ProfilingSampleInstanceOcclusionTest = new ProfilingSampler("InstanceOcclusionTest");
		m_ShaderVariables = new NativeArray<InstanceOcclusionCullerShaderVariables>(1, Allocator.Persistent);
		m_ConstantBuffer = new ComputeBuffer(1, UnsafeUtility.SizeOf<InstanceOcclusionCullerShaderVariables>(), ComputeBufferType.Constant);
		m_CommandBuffer = new CommandBuffer();
		m_CommandBuffer.name = "EnsureValidOcclusionTestResults";
		m_LODParamsToCameraID = new NativeParallelHashMap<int, AnimatedFadeData>(16, Allocator.Persistent);
	}

	private JobHandle AnimateCrossFades(CPUPerCameraInstanceData perCameraInstanceData, BatchCullingContext cc, out CPUPerCameraInstanceData.PerCameraInstanceDataArrays cameraInstanceData, out bool hasAnimatedCrossfade)
	{
		int hashCode = cc.lodParameters.GetHashCode();
		hasAnimatedCrossfade = m_LODParamsToCameraID.TryGetValue(hashCode, out var item);
		if (hasAnimatedCrossfade)
		{
			cameraInstanceData = perCameraInstanceData.perCameraData[item.cameraID];
			return item.jobHandle;
		}
		if (cc.viewType != BatchCullingViewType.Camera && !hasAnimatedCrossfade)
		{
			cameraInstanceData = default(CPUPerCameraInstanceData.PerCameraInstanceDataArrays);
			return default(JobHandle);
		}
		int instanceID = cc.viewID.GetInstanceID();
		hasAnimatedCrossfade = perCameraInstanceData.perCameraData.TryGetValue(instanceID, out var item2);
		if (!hasAnimatedCrossfade)
		{
			cameraInstanceData = default(CPUPerCameraInstanceData.PerCameraInstanceDataArrays);
			return default(JobHandle);
		}
		cameraInstanceData = item2;
		AnimateCrossFadeJob jobData = default(AnimateCrossFadeJob);
		jobData.deltaTime = Time.deltaTime;
		jobData.crossFadeArray = cameraInstanceData.crossFades;
		JobHandle jobHandle = IJobParallelForExtensions.Schedule(jobData, perCameraInstanceData.instancesLength, 512);
		m_LODParamsToCameraID.TryAdd(hashCode, new AnimatedFadeData
		{
			cameraID = instanceID,
			jobHandle = jobHandle
		});
		return jobHandle;
	}

	private unsafe JobHandle CreateFrustumCullingJob(in BatchCullingContext cc, in CPUInstanceData.ReadOnly instanceData, in CPUSharedInstanceData.ReadOnly sharedInstanceData, in CPUPerCameraInstanceData perCameraInstanceData, NativeList<LODGroupCullingData> lodGroupCullingData, in BinningConfig binningConfig, float smallMeshScreenPercentage, OcclusionCullingCommon occlusionCullingCommon, NativeArray<byte> rendererVisibilityMasks, NativeArray<byte> rendererMeshLodSettings, NativeArray<byte> rendererCrossFadeValues)
	{
		ReceiverPlanes receiverPlanes = default(ReceiverPlanes);
		ReceiverSphereCuller receiverSphereCuller = default(ReceiverSphereCuller);
		FrustumPlaneCuller frustumPlaneCuller = default(FrustumPlaneCuller);
		float num = default(float);
		float num2 = default(float);
		fixed (BatchCullingContext* context = &cc)
		{
			InstanceCullerBurst.SetupCullingJobInput(QualitySettings.lodBias, QualitySettings.meshLodThreshold, context, &receiverPlanes, &receiverSphereCuller, &frustumPlaneCuller, &num, &num2);
		}
		occlusionCullingCommon?.UpdateSilhouettePlanes(cc.viewID.GetInstanceID(), receiverPlanes.SilhouettePlaneSubArray());
		CPUPerCameraInstanceData.PerCameraInstanceDataArrays cameraInstanceData;
		bool hasAnimatedCrossfade;
		JobHandle dependsOn = AnimateCrossFades(perCameraInstanceData, cc, out cameraInstanceData, out hasAnimatedCrossfade);
		CullingJob jobData = default(CullingJob);
		jobData.binningConfig = binningConfig;
		jobData.viewType = cc.viewType;
		jobData.frustumPlanePackets = frustumPlaneCuller.planePackets.AsArray();
		jobData.frustumSplitInfos = frustumPlaneCuller.splitInfos.AsArray();
		jobData.lightFacingFrustumPlanes = receiverPlanes.LightFacingFrustumPlaneSubArray();
		jobData.receiverSplitInfos = receiverSphereCuller.splitInfos.AsArray();
		jobData.worldToLightSpaceRotation = receiverSphereCuller.worldToLightSpaceRotation;
		jobData.cullLightmappedShadowCasters = (cc.cullingFlags & BatchCullingFlags.CullLightmappedShadowCasters) != 0;
		jobData.cameraPosition = cc.lodParameters.cameraPosition;
		jobData.sqrMeshLodSelectionConstant = num2 * num2;
		jobData.sqrScreenRelativeMetric = num * num;
		jobData.minScreenRelativeHeight = smallMeshScreenPercentage * 0.01f;
		jobData.isOrtho = cc.lodParameters.isOrthographic;
		jobData.animateCrossFades = hasAnimatedCrossfade;
		jobData.instanceData = instanceData;
		jobData.sharedInstanceData = sharedInstanceData;
		jobData.cameraInstanceData = cameraInstanceData;
		jobData.lodGroupCullingData = lodGroupCullingData;
		jobData.occlusionBuffer = cc.occlusionBuffer;
		jobData.rendererVisibilityMasks = rendererVisibilityMasks;
		jobData.rendererMeshLodSettings = rendererMeshLodSettings;
		jobData.rendererCrossFadeValues = rendererCrossFadeValues;
		jobData.maxLOD = QualitySettings.maximumLODLevel;
		jobData.cullingLayerMask = cc.cullingLayerMask;
		jobData.sceneCullingMask = cc.sceneCullingMask;
		JobHandle jobHandle = IJobParallelForExtensions.Schedule(jobData, instanceData.instancesLength, 32, dependsOn);
		receiverPlanes.Dispose(jobHandle);
		frustumPlaneCuller.Dispose(jobHandle);
		receiverSphereCuller.Dispose(jobHandle);
		return jobHandle;
	}

	private int ComputeWorstCaseDrawCommandCount(in BatchCullingContext cc, BinningConfig binningConfig, CPUDrawInstanceData drawInstanceData)
	{
		int length = drawInstanceData.drawInstances.Length;
		int num = drawInstanceData.drawBatches.Length;
		if (binningConfig.supportsCrossFade)
		{
			num *= 2;
		}
		num *= 2;
		if (binningConfig.supportsMotionCheck)
		{
			num *= 2;
		}
		if (cc.cullingSplits.Length > 1)
		{
			num <<= cc.cullingSplits.Length - 1;
		}
		return math.min(num, length);
	}

	public unsafe JobHandle CreateCullJobTree(in BatchCullingContext cc, BatchCullingOutput cullingOutput, in CPUInstanceData.ReadOnly instanceData, in CPUSharedInstanceData.ReadOnly sharedInstanceData, in CPUPerCameraInstanceData perCameraInstanceData, in GPUInstanceDataBuffer.ReadOnly instanceDataBuffer, NativeList<LODGroupCullingData> lodGroupCullingData, CPUDrawInstanceData drawInstanceData, NativeParallelHashMap<uint, BatchID> batchIDs, float smallMeshScreenPercentage, OcclusionCullingCommon occlusionCullingCommon)
	{
		BatchCullingOutputDrawCommands value = default(BatchCullingOutputDrawCommands);
		value.drawRangeCount = drawInstanceData.drawRanges.Length;
		value.drawRanges = MemoryUtilities.Malloc<BatchDrawRange>(value.drawRangeCount, Allocator.TempJob);
		for (int i = 0; i < value.drawRangeCount; i++)
		{
			value.drawRanges[i].drawCommandsCount = 0u;
		}
		cullingOutput.drawCommands[0] = value;
		cullingOutput.customCullingResult[0] = IntPtr.Zero;
		BinningConfig binningConfig = default(BinningConfig);
		binningConfig.viewCount = cc.cullingSplits.Length;
		binningConfig.supportsCrossFade = QualitySettings.enableLODCrossFade;
		binningConfig.supportsMotionCheck = cc.viewType == BatchCullingViewType.Camera;
		BinningConfig binningConfig2 = binningConfig;
		int handlesLength = instanceData.handlesLength;
		NativeArray<byte> rendererVisibilityMasks = new NativeArray<byte>(handlesLength, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<byte> rendererCrossFadeValues = new NativeArray<byte>(handlesLength, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<byte> rendererMeshLodSettings = new NativeArray<byte>(handlesLength, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		JobHandle jobHandle = CreateFrustumCullingJob(in cc, in instanceData, in sharedInstanceData, in perCameraInstanceData, lodGroupCullingData, in binningConfig2, smallMeshScreenPercentage, occlusionCullingCommon, rendererVisibilityMasks, rendererMeshLodSettings, rendererCrossFadeValues);
		if (cc.viewType == BatchCullingViewType.Camera || cc.viewType == BatchCullingViewType.Light || cc.viewType == BatchCullingViewType.SelectionOutline)
		{
			jobHandle = CreateCompactedVisibilityMaskJob(in instanceData, rendererVisibilityMasks, jobHandle);
			int num = -1;
			DebugRendererBatcherStats debugStats = m_DebugStats;
			if (debugStats != null && debugStats.enabled)
			{
				num = m_SplitDebugArray.TryAddSplits(cc.viewType, cc.viewID.GetInstanceID(), cc.cullingSplits.Length);
			}
			int length = drawInstanceData.drawBatches.Length;
			int length2 = ComputeWorstCaseDrawCommandCount(in cc, binningConfig2, drawInstanceData);
			NativeArray<int> batchBinAllocOffsets = new NativeArray<int>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<int> batchBinCounts = new NativeArray<int>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<int> batchDrawCommandOffsets = new NativeArray<int>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<int> binAllocCounter = new NativeArray<int>(16, Allocator.TempJob);
			NativeArray<short> binConfigIndices = new NativeArray<short>(length2, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<int> binVisibleInstanceCounts = new NativeArray<int>(length2, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<int> binVisibleInstanceOffsets = new NativeArray<int>(length2, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			int contextIndex = -1;
			int num2;
			if (occlusionCullingCommon != null)
			{
				num2 = (occlusionCullingCommon.HasOccluderContext(cc.viewID.GetInstanceID()) ? 1 : 0);
				if (num2 != 0)
				{
					int instanceID = cc.viewID.GetInstanceID();
					contextIndex = m_IndirectStorage.TryAllocateContext(instanceID);
					cullingOutput.customCullingResult[0] = (IntPtr)instanceID;
				}
			}
			else
			{
				num2 = 0;
			}
			IndirectBufferLimits limits = m_IndirectStorage.GetLimits(contextIndex);
			NativeArray<IndirectBufferAllocInfo> allocInfoSubArray = m_IndirectStorage.GetAllocInfoSubArray(contextIndex);
			JobHandle jobHandle2 = IJobParallelForExtensions.Schedule(new AllocateBinsPerBatch
			{
				binningConfig = binningConfig2,
				drawBatches = drawInstanceData.drawBatches,
				drawInstanceIndices = drawInstanceData.drawInstanceIndices,
				instanceData = instanceData,
				rendererVisibilityMasks = rendererVisibilityMasks,
				rendererMeshLodSettings = rendererMeshLodSettings,
				batchBinAllocOffsets = batchBinAllocOffsets,
				batchBinCounts = batchBinCounts,
				binAllocCounter = binAllocCounter,
				binConfigIndices = binConfigIndices,
				binVisibleInstanceCounts = binVisibleInstanceCounts,
				splitDebugCounters = m_SplitDebugArray.Counters,
				debugCounterIndexBase = num
			}, length, 1, jobHandle);
			m_SplitDebugArray.AddSync(num, jobHandle2);
			JobHandle jobHandle3 = IJobParallelForExtensions.Schedule(dependsOn: new PrefixSumDrawsAndInstances
			{
				drawRanges = drawInstanceData.drawRanges,
				drawBatchIndices = drawInstanceData.drawBatchIndices,
				batchBinAllocOffsets = batchBinAllocOffsets,
				batchBinCounts = batchBinCounts,
				binVisibleInstanceCounts = binVisibleInstanceCounts,
				batchDrawCommandOffsets = batchDrawCommandOffsets,
				binVisibleInstanceOffsets = binVisibleInstanceOffsets,
				cullingOutput = cullingOutput.drawCommands,
				indirectBufferLimits = limits,
				indirectBufferAllocInfo = allocInfoSubArray,
				indirectAllocationCounters = m_IndirectStorage.allocationCounters
			}.Schedule(jobHandle2), jobData: new DrawCommandOutputPerBatch
			{
				binningConfig = binningConfig2,
				batchIDs = batchIDs,
				instanceDataBuffer = instanceDataBuffer,
				drawBatches = drawInstanceData.drawBatches,
				drawInstanceIndices = drawInstanceData.drawInstanceIndices,
				instanceData = instanceData,
				rendererVisibilityMasks = rendererVisibilityMasks,
				rendererMeshLodSettings = rendererMeshLodSettings,
				rendererCrossFadeValues = rendererCrossFadeValues,
				batchBinAllocOffsets = batchBinAllocOffsets,
				batchBinCounts = batchBinCounts,
				batchDrawCommandOffsets = batchDrawCommandOffsets,
				binConfigIndices = binConfigIndices,
				binVisibleInstanceOffsets = binVisibleInstanceOffsets,
				binVisibleInstanceCounts = binVisibleInstanceCounts,
				cullingOutput = cullingOutput.drawCommands,
				indirectBufferLimits = limits,
				visibleInstancesBufferHandle = m_IndirectStorage.visibleInstanceBufferHandle,
				indirectArgsBufferHandle = m_IndirectStorage.indirectArgsBufferHandle,
				indirectBufferAllocInfo = allocInfoSubArray,
				indirectInstanceInfoGlobalArray = m_IndirectStorage.instanceInfoGlobalArray,
				indirectDrawInfoGlobalArray = m_IndirectStorage.drawInfoGlobalArray
			}, arrayLength: length, innerloopBatchCount: 1);
			if (num2 != 0)
			{
				m_IndirectStorage.SetBufferContext(contextIndex, new IndirectBufferContext(jobHandle3));
			}
			jobHandle = jobHandle3;
		}
		jobHandle = rendererVisibilityMasks.Dispose(jobHandle);
		jobHandle = rendererCrossFadeValues.Dispose(jobHandle);
		return rendererMeshLodSettings.Dispose(jobHandle);
	}

	private JobHandle CreateCompactedVisibilityMaskJob(in CPUInstanceData.ReadOnly instanceData, NativeArray<byte> rendererVisibilityMasks, JobHandle cullingJobHandle)
	{
		if (!m_CompactedVisibilityMasks.IsCreated)
		{
			m_CompactedVisibilityMasks = new ParallelBitArray(instanceData.handlesLength, Allocator.TempJob);
		}
		CompactVisibilityMasksJob jobData = default(CompactVisibilityMasksJob);
		jobData.rendererVisibilityMasks = rendererVisibilityMasks;
		jobData.compactedVisibilityMasks = m_CompactedVisibilityMasks;
		JobHandle jobHandle = jobData.ScheduleBatch(rendererVisibilityMasks.Length, 64, cullingJobHandle);
		m_CompactedVisibilityMasksJobsHandle = JobHandle.CombineDependencies(m_CompactedVisibilityMasksJobsHandle, jobHandle);
		return jobHandle;
	}

	public void InstanceOccludersUpdated(int viewInstanceID, int subviewMask, RenderersBatchersContext batchersContext)
	{
		DebugRendererBatcherStats debugStats = m_DebugStats;
		if (debugStats != null && debugStats.enabled && batchersContext.occlusionCullingCommon.GetOccluderContext(viewInstanceID, out var occluderContext))
		{
			m_OcclusionEventDebugArray.TryAdd(viewInstanceID, InstanceOcclusionEventType.OccluderUpdate, occluderContext.version, subviewMask, OcclusionTest.None);
		}
	}

	private void DisposeCompactVisibilityMasks()
	{
		if (m_CompactedVisibilityMasks.IsCreated)
		{
			m_CompactedVisibilityMasks.Dispose();
		}
	}

	private void DisposeSceneViewHiddenBits()
	{
	}

	public ParallelBitArray GetCompactedVisibilityMasks(bool syncCullingJobs)
	{
		if (syncCullingJobs)
		{
			m_CompactedVisibilityMasksJobsHandle.Complete();
		}
		return m_CompactedVisibilityMasks;
	}

	public void InstanceOcclusionTest(RenderGraph renderGraph, in OcclusionCullingSettings settings, ReadOnlySpan<SubviewOcclusionTest> subviewOcclusionTests, RenderersBatchersContext batchersContext)
	{
		if (!batchersContext.occlusionCullingCommon.GetOccluderContext(settings.viewInstanceID, out var occluderContext))
		{
			return;
		}
		OccluderHandles occluderHandles = occluderContext.Import(renderGraph);
		if (!occluderHandles.IsValid())
		{
			return;
		}
		InstanceOcclusionTestPassData passData;
		using IComputeRenderGraphBuilder computeRenderGraphBuilder = renderGraph.AddComputePass<InstanceOcclusionTestPassData>("Instance Occlusion Test", out passData, m_ProfilingSampleInstanceOcclusionTest, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\GPUDriven\\InstanceCuller.cs", 2326);
		computeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		passData.settings = settings;
		passData.subviewSettings = InstanceOcclusionTestSubviewSettings.FromSpan(subviewOcclusionTests);
		passData.bufferHandles = m_IndirectStorage.ImportBuffers(renderGraph);
		passData.occluderHandles = occluderHandles;
		passData.bufferHandles.UseForOcclusionTest(computeRenderGraphBuilder);
		passData.occluderHandles.UseForOcclusionTest(computeRenderGraphBuilder);
		computeRenderGraphBuilder.SetRenderFunc(delegate(InstanceOcclusionTestPassData data, ComputeGraphContext context)
		{
			GPUResidentBatcher batcher = GPUResidentDrawer.instance.batcher;
			batcher.instanceCullingBatcher.culler.AddOcclusionCullingDispatch(context.cmd, in data.settings, in data.subviewSettings, in data.bufferHandles, in data.occluderHandles, batcher.batchersContext);
		});
	}

	internal void EnsureValidOcclusionTestResults(int viewInstanceID)
	{
		int num = m_IndirectStorage.TryGetContextIndex(viewInstanceID);
		if (num >= 0)
		{
			IndirectBufferContext bufferContext = m_IndirectStorage.GetBufferContext(num);
			if (bufferContext.bufferState == IndirectBufferContext.BufferState.Pending)
			{
				bufferContext.cullingJobHandle.Complete();
			}
			IndirectBufferAllocInfo allocInfo = m_IndirectStorage.GetAllocInfo(num);
			if (!allocInfo.IsEmpty())
			{
				CommandBuffer commandBuffer = m_CommandBuffer;
				commandBuffer.Clear();
				m_IndirectStorage.CopyFromStaging(commandBuffer, in allocInfo);
				ComputeShader cs = m_OcclusionTestShader.cs;
				m_ShaderVariables[0] = new InstanceOcclusionCullerShaderVariables
				{
					_DrawInfoAllocIndex = (uint)allocInfo.drawAllocIndex,
					_DrawInfoCount = (uint)allocInfo.drawCount,
					_InstanceInfoAllocIndex = (uint)(2 * allocInfo.instanceAllocIndex),
					_InstanceInfoCount = (uint)allocInfo.instanceCount,
					_BoundingSphereInstanceDataAddress = 0,
					_DebugCounterIndex = -1,
					_InstanceMultiplierShift = 0
				};
				commandBuffer.SetBufferData(m_ConstantBuffer, m_ShaderVariables);
				commandBuffer.SetComputeConstantBufferParam(cs, ShaderIDs.InstanceOcclusionCullerShaderVariables, m_ConstantBuffer, 0, m_ConstantBuffer.stride);
				int copyInstancesKernel = m_CopyInstancesKernel;
				commandBuffer.SetComputeBufferParam(cs, copyInstancesKernel, ShaderIDs._DrawInfo, m_IndirectStorage.drawInfoBuffer);
				commandBuffer.SetComputeBufferParam(cs, copyInstancesKernel, ShaderIDs._InstanceInfo, m_IndirectStorage.instanceInfoBuffer);
				commandBuffer.SetComputeBufferParam(cs, copyInstancesKernel, ShaderIDs._DrawArgs, m_IndirectStorage.argsBuffer);
				commandBuffer.SetComputeBufferParam(cs, copyInstancesKernel, ShaderIDs._InstanceIndices, m_IndirectStorage.instanceBuffer);
				commandBuffer.DispatchCompute(cs, copyInstancesKernel, (allocInfo.instanceCount + 63) / 64, 1, 1);
				Graphics.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();
			}
		}
	}

	private void AddOcclusionCullingDispatch(ComputeCommandBuffer cmd, in OcclusionCullingSettings settings, in InstanceOcclusionTestSubviewSettings subviewSettings, in IndirectBufferContextHandles bufferHandles, in OccluderHandles occluderHandles, RenderersBatchersContext batchersContext)
	{
		OcclusionCullingCommon occlusionCullingCommon = batchersContext.occlusionCullingCommon;
		int num = m_IndirectStorage.TryGetContextIndex(settings.viewInstanceID);
		if (num < 0)
		{
			return;
		}
		IndirectBufferContext bufferContext = m_IndirectStorage.GetBufferContext(num);
		OccluderContext occluderContext;
		bool flag = occlusionCullingCommon.GetOccluderContext(settings.viewInstanceID, out occluderContext) && (subviewSettings.occluderSubviewMask & occluderContext.subviewValidMask) == subviewSettings.occluderSubviewMask;
		IndirectBufferContext.BufferState bufferState = IndirectBufferContext.BufferState.Zeroed;
		int occluderVersion = 0;
		int subviewMask = 0;
		switch (settings.occlusionTest)
		{
		case OcclusionTest.None:
			bufferState = IndirectBufferContext.BufferState.NoOcclusionTest;
			break;
		case OcclusionTest.TestAll:
			if (flag)
			{
				bufferState = IndirectBufferContext.BufferState.AllInstancesOcclusionTested;
				occluderVersion = occluderContext.version;
				subviewMask = subviewSettings.occluderSubviewMask;
			}
			else
			{
				bufferState = IndirectBufferContext.BufferState.NoOcclusionTest;
			}
			break;
		case OcclusionTest.TestCulled:
		{
			if (!flag)
			{
				break;
			}
			bool flag2 = true;
			switch (bufferContext.bufferState)
			{
			case IndirectBufferContext.BufferState.AllInstancesOcclusionTested:
			case IndirectBufferContext.BufferState.OccludedInstancesReTested:
				if (bufferContext.subviewMask != subviewSettings.occluderSubviewMask)
				{
					Debug.Log("Expected an occlusion test of TestCulled to use the same subview mask as the previous occlusion test");
					flag2 = false;
				}
				break;
			case IndirectBufferContext.BufferState.Zeroed:
			case IndirectBufferContext.BufferState.NoOcclusionTest:
				flag2 = false;
				break;
			default:
				flag2 = false;
				Debug.Log("Expected the previous occlusion test to be TestAll before using TestCulled");
				break;
			}
			if (flag2)
			{
				bufferState = IndirectBufferContext.BufferState.OccludedInstancesReTested;
				occluderVersion = occluderContext.version;
				subviewMask = subviewSettings.occluderSubviewMask;
			}
			break;
		}
		}
		if (!bufferContext.Matches(bufferState, occluderVersion, subviewMask))
		{
			bool flag3 = bufferState == IndirectBufferContext.BufferState.AllInstancesOcclusionTested;
			bool flag4 = bufferState == IndirectBufferContext.BufferState.OccludedInstancesReTested;
			bool num2 = bufferContext.bufferState == IndirectBufferContext.BufferState.Pending;
			bool flag5 = bufferState == IndirectBufferContext.BufferState.NoOcclusionTest;
			bool flag6 = bufferContext.bufferState != IndirectBufferContext.BufferState.Zeroed && !flag5;
			bool flag7 = bufferState != IndirectBufferContext.BufferState.Zeroed && !flag5;
			if (num2)
			{
				bufferContext.cullingJobHandle.Complete();
			}
			IndirectBufferAllocInfo allocInfo = m_IndirectStorage.GetAllocInfo(num);
			bufferContext.bufferState = bufferState;
			bufferContext.occluderVersion = occluderVersion;
			bufferContext.subviewMask = subviewMask;
			if (!allocInfo.IsEmpty())
			{
				int debugCounterIndex = -1;
				DebugRendererBatcherStats debugStats = m_DebugStats;
				if (debugStats != null && debugStats.enabled)
				{
					debugCounterIndex = m_OcclusionEventDebugArray.TryAdd(settings.viewInstanceID, InstanceOcclusionEventType.OcclusionTest, occluderVersion, subviewMask, flag3 ? OcclusionTest.TestAll : (flag4 ? OcclusionTest.TestCulled : OcclusionTest.None));
				}
				bool flag8 = false;
				if (flag3 || flag4)
				{
					flag8 = OcclusionCullingCommon.UseOcclusionDebug(in occluderContext) && occluderHandles.occlusionDebugOverlay.IsValid();
				}
				ComputeShader cs = m_OcclusionTestShader.cs;
				LocalKeyword keyword = new LocalKeyword(cs, "OCCLUSION_FIRST_PASS");
				LocalKeyword keyword2 = new LocalKeyword(cs, "OCCLUSION_SECOND_PASS");
				OccluderContext.SetKeyword(cmd, cs, in keyword, flag3);
				OccluderContext.SetKeyword(cmd, cs, in keyword2, flag4);
				m_ShaderVariables[0] = new InstanceOcclusionCullerShaderVariables
				{
					_DrawInfoAllocIndex = (uint)allocInfo.drawAllocIndex,
					_DrawInfoCount = (uint)allocInfo.drawCount,
					_InstanceInfoAllocIndex = (uint)(2 * allocInfo.instanceAllocIndex),
					_InstanceInfoCount = (uint)allocInfo.instanceCount,
					_BoundingSphereInstanceDataAddress = batchersContext.renderersParameters.boundingSphere.gpuAddress,
					_DebugCounterIndex = debugCounterIndex,
					_InstanceMultiplierShift = ((settings.instanceMultiplier == 2) ? 1 : 0)
				};
				cmd.SetBufferData(m_ConstantBuffer, m_ShaderVariables);
				cmd.SetComputeConstantBufferParam(cs, ShaderIDs.InstanceOcclusionCullerShaderVariables, m_ConstantBuffer, 0, m_ConstantBuffer.stride);
				occlusionCullingCommon.PrepareCulling(cmd, in occluderContext, in settings, in subviewSettings, in m_OcclusionTestShader, flag8);
				if (flag5)
				{
					int copyInstancesKernel = m_CopyInstancesKernel;
					cmd.SetComputeBufferParam(cs, copyInstancesKernel, ShaderIDs._DrawInfo, m_IndirectStorage.drawInfoBuffer);
					cmd.SetComputeBufferParam(cs, copyInstancesKernel, ShaderIDs._InstanceInfo, m_IndirectStorage.instanceInfoBuffer);
					cmd.SetComputeBufferParam(cs, copyInstancesKernel, ShaderIDs._DrawArgs, m_IndirectStorage.argsBuffer);
					cmd.SetComputeBufferParam(cs, copyInstancesKernel, ShaderIDs._InstanceIndices, m_IndirectStorage.instanceBuffer);
					cmd.DispatchCompute(cs, copyInstancesKernel, (allocInfo.instanceCount + 63) / 64, 1, 1);
				}
				if (flag6)
				{
					int resetDrawArgsKernel = m_ResetDrawArgsKernel;
					cmd.SetComputeBufferParam(cs, resetDrawArgsKernel, ShaderIDs._DrawInfo, bufferHandles.drawInfoBuffer);
					cmd.SetComputeBufferParam(cs, resetDrawArgsKernel, ShaderIDs._DrawArgs, bufferHandles.argsBuffer);
					cmd.DispatchCompute(cs, resetDrawArgsKernel, (allocInfo.drawCount + 63) / 64, 1, 1);
				}
				if (flag7)
				{
					int cullInstancesKernel = m_CullInstancesKernel;
					cmd.SetComputeBufferParam(cs, cullInstancesKernel, ShaderIDs._DrawInfo, bufferHandles.drawInfoBuffer);
					cmd.SetComputeBufferParam(cs, cullInstancesKernel, ShaderIDs._InstanceInfo, bufferHandles.instanceInfoBuffer);
					cmd.SetComputeBufferParam(cs, cullInstancesKernel, ShaderIDs._DrawArgs, bufferHandles.argsBuffer);
					cmd.SetComputeBufferParam(cs, cullInstancesKernel, ShaderIDs._InstanceIndices, bufferHandles.instanceBuffer);
					cmd.SetComputeBufferParam(cs, cullInstancesKernel, ShaderIDs._InstanceDataBuffer, batchersContext.gpuInstanceDataBuffer);
					cmd.SetComputeBufferParam(cs, cullInstancesKernel, ShaderIDs._OcclusionDebugCounters, m_OcclusionEventDebugArray.CounterBuffer);
					if (flag3 || flag4)
					{
						OcclusionCullingCommon.SetDepthPyramid(cmd, in m_OcclusionTestShader, cullInstancesKernel, in occluderHandles);
					}
					if (flag8)
					{
						OcclusionCullingCommon.SetDebugPyramid(cmd, in m_OcclusionTestShader, cullInstancesKernel, in occluderHandles);
					}
					if (flag4)
					{
						cmd.DispatchCompute(cs, cullInstancesKernel, bufferHandles.argsBuffer, (uint)(20 * allocInfo.GetExtraDrawInfoSlotIndex()));
					}
					else
					{
						cmd.DispatchCompute(cs, cullInstancesKernel, (allocInfo.instanceCount + 63) / 64, 1, 1);
					}
				}
			}
		}
		m_IndirectStorage.SetBufferContext(num, bufferContext);
	}

	private void FlushDebugCounters()
	{
		DebugRendererBatcherStats debugStats = m_DebugStats;
		if (debugStats != null && debugStats.enabled)
		{
			m_SplitDebugArray.MoveToDebugStatsAndClear(m_DebugStats);
			m_OcclusionEventDebugArray.MoveToDebugStatsAndClear(m_DebugStats);
			m_DebugStats.FinalizeInstanceCullerViewStats();
		}
	}

	private void OnBeginSceneViewCameraRendering()
	{
	}

	private void OnEndSceneViewCameraRendering()
	{
	}

	public void UpdateFrame(int cameraCount)
	{
		DisposeCompactVisibilityMasks();
		if (cameraCount > m_LODParamsToCameraID.Capacity)
		{
			m_LODParamsToCameraID.Capacity = cameraCount;
		}
		m_LODParamsToCameraID.Clear();
		FlushDebugCounters();
		m_IndirectStorage.ClearContextsAndGrowBuffers();
	}

	public void OnBeginCameraRendering(Camera camera)
	{
		if (camera.cameraType == CameraType.SceneView)
		{
			OnBeginSceneViewCameraRendering();
		}
	}

	public void OnEndCameraRendering(Camera camera)
	{
		if (camera.cameraType == CameraType.SceneView)
		{
			OnEndSceneViewCameraRendering();
		}
	}

	public void Dispose()
	{
		DisposeSceneViewHiddenBits();
		DisposeCompactVisibilityMasks();
		m_IndirectStorage.Dispose();
		m_DebugStats = null;
		m_OcclusionEventDebugArray.Dispose();
		m_SplitDebugArray.Dispose();
		m_ShaderVariables.Dispose();
		m_ConstantBuffer.Release();
		m_CommandBuffer.Dispose();
		m_LODParamsToCameraID.Dispose();
	}
}
