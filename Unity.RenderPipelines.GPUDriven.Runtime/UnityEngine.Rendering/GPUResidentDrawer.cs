using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.SceneManagement;

namespace UnityEngine.Rendering;

public class GPUResidentDrawer
{
	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	private struct FindRenderersFromMaterialOrMeshJob : IJobParallelForBatch
	{
		public const int k_BatchSize = 128;

		[ReadOnly]
		public NativeHashSet<int>.ReadOnly materialIDs;

		[ReadOnly]
		public NativeArray<SmallIntegerArray>.ReadOnly materialIDArrays;

		[ReadOnly]
		public NativeArray<int>.ReadOnly meshIDs;

		[ReadOnly]
		public NativeArray<int>.ReadOnly meshIDArray;

		[ReadOnly]
		public NativeArray<int>.ReadOnly rendererGroupIDs;

		[ReadOnly]
		public NativeArray<int>.ReadOnly sortedExcludeRendererIDs;

		[WriteOnly]
		public NativeList<int>.ParallelWriter selectedRenderGroupsForMaterials;

		[WriteOnly]
		public NativeList<int>.ParallelWriter selectedRenderGroupsForMeshes;

		public unsafe void Execute(int startIndex, int count)
		{
			int* ptr = stackalloc int[128];
			UnsafeList<int> unsafeList = new UnsafeList<int>(ptr, 128);
			unsafeList.Length = 0;
			int* ptr2 = stackalloc int[128];
			UnsafeList<int> unsafeList2 = new UnsafeList<int>(ptr2, 128);
			unsafeList2.Length = 0;
			for (int i = 0; i < count; i++)
			{
				int index = startIndex + i;
				int value = rendererGroupIDs[index];
				if (sortedExcludeRendererIDs.BinarySearch(value) >= 0)
				{
					continue;
				}
				int value2 = meshIDArray[index];
				if (meshIDs.Contains(value2))
				{
					unsafeList2.AddNoResize(value);
					continue;
				}
				SmallIntegerArray smallIntegerArray = materialIDArrays[index];
				for (int j = 0; j < smallIntegerArray.Length; j++)
				{
					int item = smallIntegerArray[j];
					if (materialIDs.Contains(item))
					{
						unsafeList.AddNoResize(value);
						break;
					}
				}
			}
			selectedRenderGroupsForMaterials.AddRangeNoResize(ptr, unsafeList.Length);
			selectedRenderGroupsForMeshes.AddRangeNoResize(ptr2, unsafeList2.Length);
		}
	}

	private static class Strings
	{
		public static readonly string drawerModeDisabled = "GPUResidentDrawer Drawer mode is disabled. Enable it on your current RenderPipelineAsset";

		public static readonly string allowInEditModeDisabled = "GPUResidentDrawer The current mode does not allow the resident drawer. Check setting Allow In Edit Mode";

		public static readonly string notGPUResidentRenderPipeline = "GPUResidentDrawer Disabled due to current render pipeline not being of type IGPUResidentRenderPipeline";

		public static readonly string rawBufferNotSupportedByPlatform = string.Format("{0} The current platform does not support {1}", "GPUResidentDrawer", BatchBufferTarget.RawBuffer.GetType());

		public static readonly string kernelNotPresent = "GPUResidentDrawer Kernel not present, please ensure the player settings includes a supported graphics API.";

		public static readonly string batchRendererGroupShaderStrippingModeInvalid = "GPUResidentDrawer \"BatchRendererGroup Variants\" setting must be \"Keep All\".  The current setting will cause errors when building a player because all DOTS instancing shaders will be stripped To fix, modify Graphics settings and set \"BatchRendererGroup Variants\" to \"Keep All\".";

		public static readonly string visionOSNotSupported = "GPUResidentDrawer Disabled on VisionOS as it is non applicable. This platform uses a custom rendering path and doesn't go through the resident drawer.";
	}

	private static GPUResidentDrawer s_Instance;

	private IntPtr m_ContextIntPtr = IntPtr.Zero;

	private GPUResidentDrawerSettings m_Settings;

	private GPUDrivenProcessor m_GPUDrivenProcessor;

	private RenderersBatchersContext m_BatchersContext;

	private GPUResidentBatcher m_Batcher;

	private ObjectDispatcher m_Dispatcher;

	internal static GPUResidentDrawer instance => s_Instance;

	internal static bool MaintainContext { get; set; }

	internal static bool ForceOcclusion { get; set; }

	internal GPUResidentBatcher batcher => m_Batcher;

	internal GPUResidentDrawerSettings settings => m_Settings;

	public static bool IsInstanceOcclusionCullingEnabled()
	{
		if (s_Instance == null)
		{
			return false;
		}
		if (s_Instance.settings.mode != GPUResidentDrawerMode.InstancedDrawing)
		{
			return false;
		}
		if (s_Instance.settings.enableOcclusionCulling)
		{
			return true;
		}
		return false;
	}

	public static void PostCullBeginCameraRendering(RenderRequestBatcherContext context)
	{
		s_Instance?.batcher.PostCullBeginCameraRendering(context);
	}

	public static void OnSetupAmbientProbe()
	{
		s_Instance?.batcher.OnSetupAmbientProbe();
	}

	public static void InstanceOcclusionTest(RenderGraph renderGraph, in OcclusionCullingSettings settings, ReadOnlySpan<SubviewOcclusionTest> subviewOcclusionTests)
	{
		s_Instance?.batcher.InstanceOcclusionTest(renderGraph, in settings, subviewOcclusionTests);
	}

	public static void UpdateInstanceOccluders(RenderGraph renderGraph, in OccluderParameters occluderParameters, ReadOnlySpan<OccluderSubviewUpdate> occluderSubviewUpdates)
	{
		s_Instance?.batcher.UpdateInstanceOccluders(renderGraph, in occluderParameters, occluderSubviewUpdates);
	}

	public static void ReinitializeIfNeeded()
	{
	}

	public static void RenderDebugOcclusionTestOverlay(RenderGraph renderGraph, DebugDisplayGPUResidentDrawer debugSettings, int viewInstanceID, TextureHandle colorBuffer)
	{
		s_Instance?.batcher.occlusionCullingCommon.RenderDebugOcclusionTestOverlay(renderGraph, debugSettings, viewInstanceID, colorBuffer);
	}

	public static void RenderDebugOccluderOverlay(RenderGraph renderGraph, DebugDisplayGPUResidentDrawer debugSettings, Vector2 screenPos, float maxHeight, TextureHandle colorBuffer)
	{
		s_Instance?.batcher.occlusionCullingCommon.RenderDebugOccluderOverlay(renderGraph, debugSettings, screenPos, maxHeight, colorBuffer);
	}

	internal static DebugRendererBatcherStats GetDebugStats()
	{
		return s_Instance?.m_BatchersContext.debugStats;
	}

	private void InsertIntoPlayerLoop()
	{
		PlayerLoopSystem currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
		bool flag = false;
		for (int i = 0; i < currentPlayerLoop.subSystemList.Length; i++)
		{
			PlayerLoopSystem playerLoopSystem = currentPlayerLoop.subSystemList[i];
			if (flag || !(playerLoopSystem.type == typeof(PostLateUpdate)))
			{
				continue;
			}
			List<PlayerLoopSystem> list = new List<PlayerLoopSystem>();
			PlayerLoopSystem[] subSystemList = playerLoopSystem.subSystemList;
			for (int j = 0; j < subSystemList.Length; j++)
			{
				PlayerLoopSystem item = subSystemList[j];
				if (item.type == typeof(PostLateUpdate.FinishFrameRendering))
				{
					PlayerLoopSystem item2 = default(PlayerLoopSystem);
					ref PlayerLoopSystem.UpdateFunction updateDelegate = ref item2.updateDelegate;
					updateDelegate = (PlayerLoopSystem.UpdateFunction)Delegate.Combine(updateDelegate, new PlayerLoopSystem.UpdateFunction(PostPostLateUpdateStatic));
					item2.type = GetType();
					list.Add(item2);
					flag = true;
				}
				list.Add(item);
			}
			playerLoopSystem.subSystemList = list.ToArray();
			currentPlayerLoop.subSystemList[i] = playerLoopSystem;
		}
		UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
	}

	private void RemoveFromPlayerLoop()
	{
		PlayerLoopSystem currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
		for (int i = 0; i < currentPlayerLoop.subSystemList.Length; i++)
		{
			PlayerLoopSystem playerLoopSystem = currentPlayerLoop.subSystemList[i];
			if (playerLoopSystem.type != typeof(PostLateUpdate))
			{
				continue;
			}
			List<PlayerLoopSystem> list = new List<PlayerLoopSystem>();
			PlayerLoopSystem[] subSystemList = playerLoopSystem.subSystemList;
			for (int j = 0; j < subSystemList.Length; j++)
			{
				PlayerLoopSystem item = subSystemList[j];
				if (item.type != GetType())
				{
					list.Add(item);
				}
			}
			playerLoopSystem.subSystemList = list.ToArray();
			currentPlayerLoop.subSystemList[i] = playerLoopSystem;
		}
		UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
	}

	internal static bool IsEnabled()
	{
		return s_Instance != null;
	}

	internal static GPUResidentDrawerSettings GetGlobalSettingsFromRPAsset()
	{
		if (!(GraphicsSettings.currentRenderPipeline is IGPUResidentRenderPipeline { gpuResidentDrawerSettings: var gpuResidentDrawerSettings }))
		{
			return default(GPUResidentDrawerSettings);
		}
		if (IsForcedOnViaCommandLine())
		{
			gpuResidentDrawerSettings.mode = GPUResidentDrawerMode.InstancedDrawing;
		}
		if (IsOcclusionForcedOnViaCommandLine() || ForceOcclusion)
		{
			gpuResidentDrawerSettings.enableOcclusionCulling = true;
		}
		return gpuResidentDrawerSettings;
	}

	internal static bool IsForcedOnViaCommandLine()
	{
		return false;
	}

	internal static bool IsOcclusionForcedOnViaCommandLine()
	{
		return false;
	}

	internal static void Reinitialize()
	{
		Recreate(GetGlobalSettingsFromRPAsset());
	}

	private static void CleanUp()
	{
		if (s_Instance != null)
		{
			s_Instance.Dispose();
			s_Instance = null;
		}
	}

	private static void Recreate(GPUResidentDrawerSettings settings)
	{
		CleanUp();
		if (IsGPUResidentDrawerSupportedBySRP(settings, out var message, out var severity))
		{
			s_Instance = new GPUResidentDrawer(settings, 4096, 0);
		}
		else
		{
			LogMessage(message, severity);
		}
	}

	private GPUResidentDrawer(GPUResidentDrawerSettings settings, int maxInstanceCount, int maxTreeInstanceCount)
	{
		GPUResidentDrawerResources renderPipelineSettings = GraphicsSettings.GetRenderPipelineSettings<GPUResidentDrawerResources>();
		_ = GraphicsSettings.currentRenderPipeline;
		m_Settings = settings;
		RenderersBatchersContextDesc desc = RenderersBatchersContextDesc.NewDefault();
		desc.instanceNumInfo = new InstanceNumInfo(maxInstanceCount, maxTreeInstanceCount);
		desc.supportDitheringCrossFade = settings.supportDitheringCrossFade;
		desc.smallMeshScreenPercentage = settings.smallMeshScreenPercentage;
		desc.enableBoundingSpheresInstanceData = settings.enableOcclusionCulling;
		desc.enableCullerDebugStats = true;
		InstanceCullingBatcherDesc instanceCullerBatcherDesc = InstanceCullingBatcherDesc.NewDefault();
		m_GPUDrivenProcessor = new GPUDrivenProcessor();
		m_BatchersContext = new RenderersBatchersContext(in desc, m_GPUDrivenProcessor, renderPipelineSettings);
		m_Batcher = new GPUResidentBatcher(m_BatchersContext, instanceCullerBatcherDesc, m_GPUDrivenProcessor);
		m_Dispatcher = new ObjectDispatcher();
		m_Dispatcher.EnableTypeTracking<LODGroup>(ObjectDispatcher.TypeTrackingFlags.SceneObjects);
		m_Dispatcher.EnableTypeTracking<Mesh>();
		m_Dispatcher.EnableTypeTracking<Material>();
		m_Dispatcher.EnableTypeTracking<MeshRenderer>(ObjectDispatcher.TypeTrackingFlags.SceneObjects);
		m_Dispatcher.EnableTypeTracking<Camera>(ObjectDispatcher.TypeTrackingFlags.SceneObjects | ObjectDispatcher.TypeTrackingFlags.EditorOnlyObjects);
		m_Dispatcher.EnableTransformTracking<MeshRenderer>(ObjectDispatcher.TransformTrackingType.GlobalTRS);
		m_Dispatcher.EnableTransformTracking<LODGroup>(ObjectDispatcher.TransformTrackingType.GlobalTRS);
		SceneManager.sceneLoaded += OnSceneLoaded;
		RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
		RenderPipelineManager.endContextRendering += OnEndContextRendering;
		RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
		RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
		Shader.EnableKeyword("USE_LEGACY_LIGHTMAPS");
		InsertIntoPlayerLoop();
	}

	private void Dispose()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
		RenderPipelineManager.endContextRendering -= OnEndContextRendering;
		RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
		RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
		RemoveFromPlayerLoop();
		Shader.DisableKeyword("USE_LEGACY_LIGHTMAPS");
		m_Dispatcher.Dispose();
		m_Dispatcher = null;
		s_Instance = null;
		m_Batcher?.Dispose();
		m_BatchersContext.Dispose();
		m_GPUDrivenProcessor.Dispose();
		m_ContextIntPtr = IntPtr.Zero;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (mode == LoadSceneMode.Additive)
		{
			m_BatchersContext.UpdateAmbientProbeAndGpuBuffer(forceUpdate: true);
		}
	}

	private static void PostPostLateUpdateStatic()
	{
		s_Instance?.PostPostLateUpdate();
	}

	private void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras)
	{
		if (s_Instance != null && m_ContextIntPtr == IntPtr.Zero)
		{
			m_ContextIntPtr = context.Internal_GetPtr();
			m_Batcher.OnBeginContextRendering();
		}
	}

	private void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
	{
		if (s_Instance != null && m_ContextIntPtr == context.Internal_GetPtr())
		{
			m_ContextIntPtr = IntPtr.Zero;
			m_Batcher.OnEndContextRendering();
		}
	}

	private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		m_Batcher.OnBeginCameraRendering(camera);
	}

	private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		m_Batcher.OnEndCameraRendering(camera);
	}

	private void PostPostLateUpdate()
	{
		m_BatchersContext.UpdateAmbientProbeAndGpuBuffer(forceUpdate: false);
		TransformDispatchData transformChangesAndClear = m_Dispatcher.GetTransformChangesAndClear<LODGroup>(ObjectDispatcher.TransformTrackingType.GlobalTRS, Allocator.TempJob);
		TypeDispatchData typeChangesAndClear = m_Dispatcher.GetTypeChangesAndClear<LODGroup>(Allocator.TempJob, sortByInstanceID: false, noScriptingArray: true);
		TypeDispatchData typeChangesAndClear2 = m_Dispatcher.GetTypeChangesAndClear<Mesh>(Allocator.TempJob, sortByInstanceID: true, noScriptingArray: true);
		TypeDispatchData typeChangesAndClear3 = m_Dispatcher.GetTypeChangesAndClear<Camera>(Allocator.TempJob, sortByInstanceID: false, noScriptingArray: true);
		TypeDispatchData typeChangesAndClear4 = m_Dispatcher.GetTypeChangesAndClear<Material>(Allocator.TempJob, sortByInstanceID: false, noScriptingArray: true);
		TypeDispatchData typeChangesAndClear5 = m_Dispatcher.GetTypeChangesAndClear<MeshRenderer>(Allocator.TempJob, sortByInstanceID: false, noScriptingArray: true);
		ClassifyMaterials(typeChangesAndClear4.changedID, out var unsupportedMaterials, out var supportedMaterials, out var supportedPackedMaterialDatas, Allocator.TempJob);
		NativeList<int> nativeList = FindUnsupportedRenderers(unsupportedMaterials.AsArray());
		ProcessMaterials(typeChangesAndClear4.destroyedID, unsupportedMaterials.AsArray());
		ProcessMeshes(typeChangesAndClear2.destroyedID);
		ProcessLODGroups(typeChangesAndClear.changedID, typeChangesAndClear.destroyedID, transformChangesAndClear.transformedID);
		ProcessCameras(typeChangesAndClear3.changedID, typeChangesAndClear3.destroyedID);
		ProcessRenderers(typeChangesAndClear5, nativeList.AsArray());
		ProcessRendererMaterialAndMeshChanges(typeChangesAndClear5.changedID, supportedMaterials.AsArray(), supportedPackedMaterialDatas.AsArray(), typeChangesAndClear2.changedID);
		transformChangesAndClear.Dispose();
		typeChangesAndClear.Dispose();
		typeChangesAndClear2.Dispose();
		typeChangesAndClear4.Dispose();
		typeChangesAndClear3.Dispose();
		typeChangesAndClear5.Dispose();
		unsupportedMaterials.Dispose();
		nativeList.Dispose();
		supportedMaterials.Dispose();
		supportedPackedMaterialDatas.Dispose();
		m_BatchersContext.UpdateInstanceMotions();
		m_Batcher.UpdateFrame();
	}

	private void ProcessMaterials(NativeArray<int> destroyedID, NativeArray<int> unsupportedMaterials)
	{
		if (destroyedID.Length > 0)
		{
			m_Batcher.DestroyMaterials(destroyedID);
		}
		if (unsupportedMaterials.Length > 0)
		{
			m_Batcher.DestroyMaterials(unsupportedMaterials);
		}
	}

	private void ProcessCameras(NativeArray<int> changedIDs, NativeArray<int> destroyedIDs)
	{
		m_BatchersContext.UpdateCameras(changedIDs);
		m_BatchersContext.FreePerCameraInstanceData(destroyedIDs);
	}

	private void ProcessMeshes(NativeArray<int> destroyedID)
	{
		if (destroyedID.Length != 0)
		{
			NativeList<InstanceHandle> instances = new NativeList<InstanceHandle>(Allocator.TempJob);
			ScheduleQueryMeshInstancesJob(destroyedID, instances).Complete();
			m_Batcher.DestroyDrawInstances(instances.AsArray());
			instances.Dispose();
			m_Batcher.DestroyMeshes(destroyedID);
		}
	}

	private void ProcessLODGroups(NativeArray<int> changedID, NativeArray<int> destroyed, NativeArray<int> transformedID)
	{
		m_BatchersContext.DestroyLODGroups(destroyed);
		m_BatchersContext.UpdateLODGroups(changedID);
		m_BatchersContext.TransformLODGroups(transformedID);
	}

	private void ProcessRendererMaterialAndMeshChanges(NativeArray<int> excludedRenderers, NativeArray<int> changedMaterials, NativeArray<GPUDrivenPackedMaterialData> changedPackedMaterialDatas, NativeArray<int> changedMeshes)
	{
		if (changedMaterials.Length == 0 && changedMeshes.Length == 0)
		{
			return;
		}
		NativeHashSet<int> materialsWithChangedPackedMaterial = GetMaterialsWithChangedPackedMaterial(changedMaterials, changedPackedMaterialDatas, Allocator.TempJob);
		JobHandle jobHandle = m_Batcher.SchedulePackedMaterialCacheUpdate(changedMaterials, changedPackedMaterialDatas);
		if (materialsWithChangedPackedMaterial.Count == 0 && changedMeshes.Length == 0)
		{
			materialsWithChangedPackedMaterial.Dispose();
			jobHandle.Complete();
			return;
		}
		NativeArray<int> nativeArray = new NativeArray<int>(excludedRenderers, Allocator.TempJob);
		if (nativeArray.Length > 0)
		{
			nativeArray.ParallelSort().Complete();
		}
		var (nativeList, nativeList2) = FindRenderersFromMaterialsOrMeshes(nativeArray, materialsWithChangedPackedMaterial, changedMeshes, Allocator.TempJob);
		materialsWithChangedPackedMaterial.Dispose();
		nativeArray.Dispose();
		jobHandle.Complete();
		if (nativeList.Length == 0 && nativeList2.Length == 0)
		{
			nativeList.Dispose();
			nativeList2.Dispose();
			return;
		}
		int length = nativeList.Length;
		int length2 = nativeList2.Length;
		int length3 = length + length2;
		NativeArray<InstanceHandle> instances = new NativeArray<InstanceHandle>(length3, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<int> nativeArray2 = new NativeArray<int>(length3, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<int>.Copy(nativeList.AsArray(), nativeArray2, length);
		NativeArray<int>.Copy(nativeList2.AsArray(), nativeArray2.GetSubArray(length, length2), length2);
		ScheduleQueryRendererGroupInstancesJob(nativeArray2, instances).Complete();
		m_Batcher.DestroyDrawInstances(instances);
		m_Batcher.UpdateRenderers(nativeList.AsArray(), materialUpdateOnly: true);
		m_Batcher.UpdateRenderers(nativeList2.AsArray());
		nativeList.Dispose();
		nativeList2.Dispose();
	}

	private void ProcessRenderers(TypeDispatchData rendererChanges, NativeArray<int> unsupportedRenderers)
	{
		NativeArray<InstanceHandle> instances = new NativeArray<InstanceHandle>(rendererChanges.changedID.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		ScheduleQueryRendererGroupInstancesJob(rendererChanges.changedID, instances).Complete();
		m_Batcher.DestroyDrawInstances(instances);
		instances.Dispose();
		m_Batcher.UpdateRenderers(rendererChanges.changedID);
		FreeRendererGroupInstances(rendererChanges.destroyedID, unsupportedRenderers);
		TransformDispatchData transformChangesAndClear = m_Dispatcher.GetTransformChangesAndClear<MeshRenderer>(ObjectDispatcher.TransformTrackingType.GlobalTRS, Allocator.TempJob);
		NativeArray<InstanceHandle> instances2 = new NativeArray<InstanceHandle>(transformChangesAndClear.transformedID.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		ScheduleQueryRendererGroupInstancesJob(transformChangesAndClear.transformedID, instances2).Complete();
		TransformInstances(instances2, transformChangesAndClear.localToWorldMatrices);
		instances2.Dispose();
		transformChangesAndClear.Dispose();
	}

	private void TransformInstances(NativeArray<InstanceHandle> instances, NativeArray<Matrix4x4> localToWorldMatrices)
	{
		m_BatchersContext.UpdateInstanceTransforms(instances, localToWorldMatrices);
	}

	private void FreeInstances(NativeArray<InstanceHandle> instances)
	{
		m_Batcher.DestroyDrawInstances(instances);
		m_BatchersContext.FreeInstances(instances);
	}

	private void FreeRendererGroupInstances(NativeArray<int> rendererGroupIDs, NativeArray<int> unsupportedRendererGroupIDs)
	{
		m_Batcher.FreeRendererGroupInstances(rendererGroupIDs);
		if (unsupportedRendererGroupIDs.Length > 0)
		{
			m_Batcher.FreeRendererGroupInstances(unsupportedRendererGroupIDs);
			m_GPUDrivenProcessor.DisableGPUDrivenRendering(unsupportedRendererGroupIDs);
		}
	}

	private InstanceHandle AppendNewInstance(int rendererGroupID, in Matrix4x4 instanceTransform)
	{
		throw new NotImplementedException();
	}

	private JobHandle ScheduleQueryRendererGroupInstancesJob(NativeArray<int> rendererGroupIDs, NativeArray<InstanceHandle> instances)
	{
		return m_BatchersContext.ScheduleQueryRendererGroupInstancesJob(rendererGroupIDs, instances);
	}

	private JobHandle ScheduleQueryRendererGroupInstancesJob(NativeArray<int> rendererGroupIDs, NativeList<InstanceHandle> instances)
	{
		return m_BatchersContext.ScheduleQueryRendererGroupInstancesJob(rendererGroupIDs, instances);
	}

	private JobHandle ScheduleQueryRendererGroupInstancesJob(NativeArray<int> rendererGroupIDs, NativeArray<int> instancesOffset, NativeArray<int> instancesCount, NativeList<InstanceHandle> instances)
	{
		return m_BatchersContext.ScheduleQueryRendererGroupInstancesJob(rendererGroupIDs, instancesOffset, instancesCount, instances);
	}

	private JobHandle ScheduleQueryMeshInstancesJob(NativeArray<int> sortedMeshIDs, NativeList<InstanceHandle> instances)
	{
		return m_BatchersContext.ScheduleQueryMeshInstancesJob(sortedMeshIDs, instances);
	}

	private void ClassifyMaterials(NativeArray<int> materials, out NativeList<int> unsupportedMaterials, out NativeList<int> supportedMaterials, out NativeList<GPUDrivenPackedMaterialData> supportedPackedMaterialDatas, Allocator allocator)
	{
		supportedMaterials = new NativeList<int>(materials.Length, allocator);
		unsupportedMaterials = new NativeList<int>(materials.Length, allocator);
		supportedPackedMaterialDatas = new NativeList<GPUDrivenPackedMaterialData>(materials.Length, allocator);
		if (materials.Length > 0)
		{
			NativeParallelHashMap<int, BatchMaterialID>.ReadOnly batchMaterialHash = m_Batcher.instanceCullingBatcher.batchMaterialHash.AsReadOnly();
			GPUResidentDrawerBurst.ClassifyMaterials(in materials, in batchMaterialHash, ref supportedMaterials, ref unsupportedMaterials, ref supportedPackedMaterialDatas);
		}
	}

	private NativeList<int> FindUnsupportedRenderers(NativeArray<int> unsupportedMaterials)
	{
		NativeList<int> unsupportedRenderers = new NativeList<int>(Allocator.TempJob);
		if (unsupportedMaterials.Length > 0)
		{
			CPUSharedInstanceData.ReadOnly sharedInstanceData = m_BatchersContext.sharedInstanceData;
			ref readonly NativeArray<SmallIntegerArray>.ReadOnly materialIDArrays = ref sharedInstanceData.materialIDArrays;
			CPUSharedInstanceData.ReadOnly sharedInstanceData2 = m_BatchersContext.sharedInstanceData;
			GPUResidentDrawerBurst.FindUnsupportedRenderers(in unsupportedMaterials, in materialIDArrays, in sharedInstanceData2.rendererGroupIDs, ref unsupportedRenderers);
		}
		return unsupportedRenderers;
	}

	private NativeHashSet<int> GetMaterialsWithChangedPackedMaterial(NativeArray<int> materials, NativeArray<GPUDrivenPackedMaterialData> packedMaterialDatas, Allocator allocator)
	{
		NativeHashSet<int> filteredMaterials = new NativeHashSet<int>(materials.Length, allocator);
		NativeParallelHashMap<int, GPUDrivenPackedMaterialData>.ReadOnly packedMaterialHash = batcher.instanceCullingBatcher.packedMaterialHash.AsReadOnly();
		GPUResidentDrawerBurst.GetMaterialsWithChangedPackedMaterial(in materials, in packedMaterialDatas, in packedMaterialHash, ref filteredMaterials);
		return filteredMaterials;
	}

	private (NativeList<int> renderersWithMaterials, NativeList<int> renderersWithMeshes) FindRenderersFromMaterialsOrMeshes(NativeArray<int> sortedExcludeRenderers, NativeHashSet<int> materials, NativeArray<int> meshes, Allocator rendererListAllocator)
	{
		CPUSharedInstanceData.ReadOnly sharedInstanceData = m_BatchersContext.sharedInstanceData;
		NativeList<int> item = new NativeList<int>(sharedInstanceData.rendererGroupIDs.Length, rendererListAllocator);
		NativeList<int> item2 = new NativeList<int>(sharedInstanceData.rendererGroupIDs.Length, rendererListAllocator);
		FindRenderersFromMaterialOrMeshJob jobData = default(FindRenderersFromMaterialOrMeshJob);
		jobData.materialIDs = materials.AsReadOnly();
		jobData.materialIDArrays = sharedInstanceData.materialIDArrays;
		jobData.meshIDs = meshes.AsReadOnly();
		jobData.meshIDArray = sharedInstanceData.meshIDs;
		jobData.rendererGroupIDs = sharedInstanceData.rendererGroupIDs;
		jobData.sortedExcludeRendererIDs = sortedExcludeRenderers.AsReadOnly();
		jobData.selectedRenderGroupsForMaterials = item.AsParallelWriter();
		jobData.selectedRenderGroupsForMeshes = item2.AsParallelWriter();
		jobData.ScheduleBatch(sharedInstanceData.rendererGroupIDs.Length, 128).Complete();
		return (renderersWithMaterials: item, renderersWithMeshes: item2);
	}

	internal static bool IsProjectSupported()
	{
		string message;
		LogType severity;
		return IsProjectSupported(out message, out severity);
	}

	internal static bool IsProjectSupported(out string message, out LogType severity)
	{
		message = string.Empty;
		severity = LogType.Log;
		if (Application.platform == RuntimePlatform.VisionOS)
		{
			message = Strings.visionOSNotSupported;
			severity = LogType.Log;
			return false;
		}
		if (BatchRendererGroup.BufferTarget != BatchBufferTarget.RawBuffer)
		{
			severity = LogType.Warning;
			message = Strings.rawBufferNotSupportedByPlatform;
			return false;
		}
		return true;
	}

	internal static bool IsGPUResidentDrawerSupportedBySRP(GPUResidentDrawerSettings settings, out string message, out LogType severity)
	{
		message = string.Empty;
		severity = LogType.Log;
		if (settings.mode == GPUResidentDrawerMode.Disabled)
		{
			message = Strings.drawerModeDisabled;
			return false;
		}
		if (IsForcedOnViaCommandLine() || MaintainContext)
		{
			return true;
		}
		if (!(GraphicsSettings.currentRenderPipeline is IGPUResidentRenderPipeline iGPUResidentRenderPipeline))
		{
			message = Strings.notGPUResidentRenderPipeline;
			severity = LogType.Warning;
			return false;
		}
		if (iGPUResidentRenderPipeline.IsGPUResidentDrawerSupportedBySRP(out message, out severity))
		{
			return IsProjectSupported(out message, out severity);
		}
		return false;
	}

	internal static void LogMessage(string message, LogType severity)
	{
		switch (severity)
		{
		case LogType.Error:
		case LogType.Exception:
			Debug.LogError(message);
			break;
		case LogType.Warning:
			Debug.LogWarning(message);
			break;
		case LogType.Assert:
		case LogType.Log:
			break;
		}
	}
}
