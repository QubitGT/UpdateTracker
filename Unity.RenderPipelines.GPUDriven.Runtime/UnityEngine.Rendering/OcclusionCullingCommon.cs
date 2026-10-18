using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering;

internal class OcclusionCullingCommon : IDisposable
{
	private struct OccluderContextSlot
	{
		public bool valid;

		public int lastUsedFrameIndex;

		public int viewInstanceID;
	}

	private static class ShaderIDs
	{
		public static readonly int OcclusionCullingCommonShaderVariables = Shader.PropertyToID("OcclusionCullingCommonShaderVariables");

		public static readonly int _OccluderDepthPyramid = Shader.PropertyToID("_OccluderDepthPyramid");

		public static readonly int _OcclusionDebugOverlay = Shader.PropertyToID("_OcclusionDebugOverlay");

		public static readonly int OcclusionCullingDebugShaderVariables = Shader.PropertyToID("OcclusionCullingDebugShaderVariables");
	}

	private class OcclusionTestOverlaySetupPassData
	{
		public OcclusionCullingDebugShaderVariables cb;
	}

	private class OcclusionTestOverlayPassData
	{
		public BufferHandle debugPyramid;
	}

	private struct DebugOccluderViewData
	{
		public int passIndex;

		public Rect viewport;

		public bool valid;
	}

	private class OccluderOverlayPassData
	{
		public Material debugMaterial;

		public RTHandle occluderTexture;

		public Rect viewport;

		public int passIndex;

		public Vector2 validRange;
	}

	private class UpdateOccludersPassData
	{
		public OccluderParameters occluderParams;

		public List<OccluderSubviewUpdate> occluderSubviewUpdates;

		public OccluderHandles occluderHandles;
	}

	private static readonly int s_MaxContextGCFrame = 8;

	private Material m_DebugOcclusionTestMaterial;

	private Material m_OccluderDebugViewMaterial;

	private ComputeShader m_OcclusionDebugCS;

	private int m_ClearOcclusionDebugKernel;

	private ComputeShader m_OccluderDepthPyramidCS;

	private int m_OccluderDepthDownscaleKernel;

	private int m_FrameIndex;

	private SilhouettePlaneCache m_SilhouettePlaneCache;

	private NativeParallelHashMap<int, int> m_ViewIDToIndexMap;

	private List<OccluderContext> m_OccluderContextData;

	private NativeList<OccluderContextSlot> m_OccluderContextSlots;

	private NativeList<int> m_FreeOccluderContexts;

	private NativeArray<OcclusionCullingCommonShaderVariables> m_CommonShaderVariables;

	private ComputeBuffer m_CommonConstantBuffer;

	private NativeArray<OcclusionCullingDebugShaderVariables> m_DebugShaderVariables;

	private ComputeBuffer m_DebugConstantBuffer;

	private ProfilingSampler m_ProfilingSamplerUpdateOccluders;

	private ProfilingSampler m_ProfilingSamplerOcclusionTestOverlay;

	private ProfilingSampler m_ProfilingSamplerOccluderOverlay;

	internal void Init(GPUResidentDrawerResources resources)
	{
		m_DebugOcclusionTestMaterial = CoreUtils.CreateEngineMaterial(resources.debugOcclusionTestPS);
		m_OccluderDebugViewMaterial = CoreUtils.CreateEngineMaterial(resources.debugOccluderPS);
		m_OcclusionDebugCS = resources.occlusionCullingDebugKernels;
		m_ClearOcclusionDebugKernel = m_OcclusionDebugCS.FindKernel("ClearOcclusionDebug");
		m_OccluderDepthPyramidCS = resources.occluderDepthPyramidKernels;
		m_OccluderDepthDownscaleKernel = m_OccluderDepthPyramidCS.FindKernel("OccluderDepthDownscale");
		m_SilhouettePlaneCache.Init();
		m_ViewIDToIndexMap = new NativeParallelHashMap<int, int>(64, Allocator.Persistent);
		m_OccluderContextData = new List<OccluderContext>();
		m_OccluderContextSlots = new NativeList<OccluderContextSlot>(64, Allocator.Persistent);
		m_FreeOccluderContexts = new NativeList<int>(64, Allocator.Persistent);
		m_ProfilingSamplerUpdateOccluders = new ProfilingSampler("UpdateOccluders");
		m_ProfilingSamplerOcclusionTestOverlay = new ProfilingSampler("OcclusionTestOverlay");
		m_ProfilingSamplerOccluderOverlay = new ProfilingSampler("OccluderOverlay");
		m_CommonShaderVariables = new NativeArray<OcclusionCullingCommonShaderVariables>(1, Allocator.Persistent);
		m_CommonConstantBuffer = new ComputeBuffer(1, UnsafeUtility.SizeOf<OcclusionCullingCommonShaderVariables>(), ComputeBufferType.Constant);
		m_DebugShaderVariables = new NativeArray<OcclusionCullingDebugShaderVariables>(1, Allocator.Persistent);
		m_DebugConstantBuffer = new ComputeBuffer(1, UnsafeUtility.SizeOf<OcclusionCullingDebugShaderVariables>(), ComputeBufferType.Constant);
	}

	internal static bool UseOcclusionDebug(in OccluderContext occluderCtx)
	{
		return occluderCtx.occlusionDebugOverlaySize != 0;
	}

	internal void PrepareCulling(ComputeCommandBuffer cmd, in OccluderContext occluderCtx, in OcclusionCullingSettings settings, in InstanceOcclusionTestSubviewSettings subviewSettings, in OcclusionTestComputeShader shader, bool useOcclusionDebug)
	{
		OccluderContext.SetKeyword(cmd, shader.cs, in shader.occlusionDebugKeyword, useOcclusionDebug);
		DebugRendererBatcherStats debugStats = GPUResidentDrawer.GetDebugStats();
		m_CommonShaderVariables[0] = new OcclusionCullingCommonShaderVariables(in occluderCtx, in subviewSettings, debugStats?.occlusionOverlayCountVisible ?? false, debugStats?.overrideOcclusionTestToAlwaysPass ?? false);
		cmd.SetBufferData(m_CommonConstantBuffer, m_CommonShaderVariables);
		cmd.SetComputeConstantBufferParam(shader.cs, ShaderIDs.OcclusionCullingCommonShaderVariables, m_CommonConstantBuffer, 0, m_CommonConstantBuffer.stride);
		DispatchDebugClear(cmd, settings.viewInstanceID);
	}

	internal static void SetDepthPyramid(ComputeCommandBuffer cmd, in OcclusionTestComputeShader shader, int kernel, in OccluderHandles occluderHandles)
	{
		cmd.SetComputeTextureParam(shader.cs, kernel, ShaderIDs._OccluderDepthPyramid, occluderHandles.occluderDepthPyramid);
	}

	internal static void SetDebugPyramid(ComputeCommandBuffer cmd, in OcclusionTestComputeShader shader, int kernel, in OccluderHandles occluderHandles)
	{
		cmd.SetComputeBufferParam(shader.cs, kernel, ShaderIDs._OcclusionDebugOverlay, occluderHandles.occlusionDebugOverlay);
	}

	public void RenderDebugOcclusionTestOverlay(RenderGraph renderGraph, DebugDisplayGPUResidentDrawer debugSettings, int viewInstanceID, TextureHandle colorBuffer)
	{
		if (debugSettings == null || !debugSettings.occlusionTestOverlayEnable)
		{
			return;
		}
		OcclusionCullingDebugOutput occlusionTestDebugOutput = GetOcclusionTestDebugOutput(viewInstanceID);
		if (occlusionTestDebugOutput.occlusionDebugOverlay == null)
		{
			return;
		}
		OcclusionTestOverlaySetupPassData passData;
		using (IComputeRenderGraphBuilder computeRenderGraphBuilder = renderGraph.AddComputePass<OcclusionTestOverlaySetupPassData>("OcclusionTestOverlay", out passData, m_ProfilingSamplerOcclusionTestOverlay, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\GPUDriven\\OcclusionCullingCommon.cs", 275))
		{
			computeRenderGraphBuilder.AllowPassCulling(value: false);
			passData.cb = occlusionTestDebugOutput.cb;
			computeRenderGraphBuilder.SetRenderFunc(delegate(OcclusionTestOverlaySetupPassData data, ComputeGraphContext ctx)
			{
				OcclusionCullingCommon occlusionCullingCommon = GPUResidentDrawer.instance.batcher.occlusionCullingCommon;
				occlusionCullingCommon.m_DebugShaderVariables[0] = data.cb;
				ctx.cmd.SetBufferData(occlusionCullingCommon.m_DebugConstantBuffer, occlusionCullingCommon.m_DebugShaderVariables);
				occlusionCullingCommon.m_DebugOcclusionTestMaterial.SetConstantBuffer(ShaderIDs.OcclusionCullingDebugShaderVariables, occlusionCullingCommon.m_DebugConstantBuffer, 0, occlusionCullingCommon.m_DebugConstantBuffer.stride);
			});
		}
		OcclusionTestOverlayPassData passData2;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<OcclusionTestOverlayPassData>("OcclusionTestOverlay", out passData2, m_ProfilingSamplerOcclusionTestOverlay, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\GPUDriven\\OcclusionCullingCommon.cs", 297);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		passData2.debugPyramid = renderGraph.ImportBuffer(occlusionTestDebugOutput.occlusionDebugOverlay);
		rasterRenderGraphBuilder.SetRenderAttachment(colorBuffer, 0);
		rasterRenderGraphBuilder.UseBuffer(in passData2.debugPyramid);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(OcclusionTestOverlayPassData data, RasterGraphContext ctx)
		{
			ctx.cmd.SetGlobalBuffer(ShaderIDs._OcclusionDebugOverlay, data.debugPyramid);
			CoreUtils.DrawFullScreen(ctx.cmd, m_DebugOcclusionTestMaterial);
		});
	}

	public void RenderDebugOccluderOverlay(RenderGraph renderGraph, DebugDisplayGPUResidentDrawer debugSettings, Vector2 screenPos, float maxHeight, TextureHandle colorBuffer)
	{
		if (debugSettings == null || !debugSettings.occluderDebugViewEnable || !debugSettings.GetOccluderViewInstanceID(out var viewInstanceID))
		{
			return;
		}
		RTHandle occluderDepthPyramid = GetOcclusionTestDebugOutput(viewInstanceID).occluderDepthPyramid;
		if (occluderDepthPyramid == null)
		{
			return;
		}
		Material occluderDebugViewMaterial = m_OccluderDebugViewMaterial;
		int passIndex = occluderDebugViewMaterial.FindPass("DebugOccluder");
		Vector2 vector = occluderDepthPyramid.referenceSize;
		float num = maxHeight / vector.y;
		vector *= num;
		Rect viewport = new Rect(screenPos.x, screenPos.y, vector.x, vector.y);
		OccluderOverlayPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<OccluderOverlayPassData>("OccluderOverlay", out passData, m_ProfilingSamplerOccluderOverlay, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\GPUDriven\\OcclusionCullingCommon.cs", 353);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderAttachment(colorBuffer, 0);
		passData.debugMaterial = occluderDebugViewMaterial;
		passData.occluderTexture = occluderDepthPyramid;
		passData.viewport = viewport;
		passData.passIndex = passIndex;
		passData.validRange = debugSettings.occluderDebugViewRange;
		rasterRenderGraphBuilder.SetRenderFunc(delegate(OccluderOverlayPassData data, RasterGraphContext ctx)
		{
			MaterialPropertyBlock tempMaterialPropertyBlock = ctx.renderGraphPool.GetTempMaterialPropertyBlock();
			tempMaterialPropertyBlock.SetTexture("_OccluderTexture", data.occluderTexture);
			tempMaterialPropertyBlock.SetVector("_ValidRange", data.validRange);
			ctx.cmd.SetViewport(data.viewport);
			ctx.cmd.DrawProcedural(Matrix4x4.identity, data.debugMaterial, data.passIndex, MeshTopology.Triangles, 3, 1, tempMaterialPropertyBlock);
		});
	}

	private void DispatchDebugClear(ComputeCommandBuffer cmd, int viewInstanceID)
	{
		if (m_ViewIDToIndexMap.TryGetValue(viewInstanceID, out var item))
		{
			OccluderContext occluderCtx = m_OccluderContextData[item];
			if (UseOcclusionDebug(in occluderCtx) && occluderCtx.debugNeedsClear)
			{
				ComputeShader occlusionDebugCS = m_OcclusionDebugCS;
				int clearOcclusionDebugKernel = m_ClearOcclusionDebugKernel;
				cmd.SetComputeConstantBufferParam(occlusionDebugCS, ShaderIDs.OcclusionCullingCommonShaderVariables, m_CommonConstantBuffer, 0, m_CommonConstantBuffer.stride);
				cmd.SetComputeBufferParam(occlusionDebugCS, clearOcclusionDebugKernel, ShaderIDs._OcclusionDebugOverlay, occluderCtx.occlusionDebugOverlay);
				Vector2Int size = occluderCtx.occluderMipBounds[0].size;
				cmd.DispatchCompute(occlusionDebugCS, clearOcclusionDebugKernel, (size.x + 7) / 8, (size.y + 7) / 8, occluderCtx.subviewCount);
				occluderCtx.debugNeedsClear = false;
				m_OccluderContextData[item] = occluderCtx;
			}
		}
	}

	private OccluderHandles PrepareOccluders(RenderGraph renderGraph, in OccluderParameters occluderParams)
	{
		OccluderHandles result = default(OccluderHandles);
		if (occluderParams.depthTexture.IsValid())
		{
			if (!m_ViewIDToIndexMap.TryGetValue(occluderParams.viewInstanceID, out var item))
			{
				item = NewContext(occluderParams.viewInstanceID);
			}
			OccluderContext value = m_OccluderContextData[item];
			value.PrepareOccluders(in occluderParams);
			result = value.Import(renderGraph);
			m_OccluderContextData[item] = value;
		}
		else
		{
			DeleteContext(occluderParams.viewInstanceID);
		}
		return result;
	}

	private void CreateFarDepthPyramid(ComputeCommandBuffer cmd, in OccluderParameters occluderParams, ReadOnlySpan<OccluderSubviewUpdate> occluderSubviewUpdates, in OccluderHandles occluderHandles)
	{
		if (m_ViewIDToIndexMap.TryGetValue(occluderParams.viewInstanceID, out var item))
		{
			NativeArray<Plane> subArray = m_SilhouettePlaneCache.GetSubArray(occluderParams.viewInstanceID);
			OccluderContext value = m_OccluderContextData[item];
			value.CreateFarDepthPyramid(cmd, in occluderParams, occluderSubviewUpdates, in occluderHandles, subArray, m_OccluderDepthPyramidCS, m_OccluderDepthDownscaleKernel);
			value.version++;
			m_OccluderContextData[item] = value;
			OccluderContextSlot value2 = m_OccluderContextSlots[item];
			value2.lastUsedFrameIndex = m_FrameIndex;
			m_OccluderContextSlots[item] = value2;
		}
	}

	public bool UpdateInstanceOccluders(RenderGraph renderGraph, in OccluderParameters occluderParams, ReadOnlySpan<OccluderSubviewUpdate> occluderSubviewUpdates)
	{
		OccluderHandles occluderHandles = PrepareOccluders(renderGraph, in occluderParams);
		if (!occluderHandles.occluderDepthPyramid.IsValid())
		{
			return false;
		}
		UpdateOccludersPassData passData;
		using (IComputeRenderGraphBuilder computeRenderGraphBuilder = renderGraph.AddComputePass<UpdateOccludersPassData>("Update Occluders", out passData, m_ProfilingSamplerUpdateOccluders, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\GPUDriven\\OcclusionCullingCommon.cs", 454))
		{
			computeRenderGraphBuilder.AllowGlobalStateModification(value: true);
			passData.occluderParams = occluderParams;
			if (passData.occluderSubviewUpdates == null)
			{
				passData.occluderSubviewUpdates = new List<OccluderSubviewUpdate>();
			}
			else
			{
				passData.occluderSubviewUpdates.Clear();
			}
			for (int i = 0; i < occluderSubviewUpdates.Length; i++)
			{
				passData.occluderSubviewUpdates.Add(occluderSubviewUpdates[i]);
			}
			passData.occluderHandles = occluderHandles;
			computeRenderGraphBuilder.UseTexture(in passData.occluderParams.depthTexture);
			passData.occluderHandles.UseForOccluderUpdate(computeRenderGraphBuilder);
			computeRenderGraphBuilder.SetRenderFunc(delegate(UpdateOccludersPassData data, ComputeGraphContext context)
			{
				Span<OccluderSubviewUpdate> span = stackalloc OccluderSubviewUpdate[data.occluderSubviewUpdates.Count];
				int num = 0;
				for (int j = 0; j < data.occluderSubviewUpdates.Count; j++)
				{
					span[j] = data.occluderSubviewUpdates[j];
					num |= 1 << data.occluderSubviewUpdates[j].subviewIndex;
				}
				GPUResidentBatcher batcher = GPUResidentDrawer.instance.batcher;
				batcher.occlusionCullingCommon.CreateFarDepthPyramid(context.cmd, in data.occluderParams, span, in data.occluderHandles);
				batcher.instanceCullingBatcher.InstanceOccludersUpdated(data.occluderParams.viewInstanceID, num);
			});
		}
		return true;
	}

	internal void UpdateSilhouettePlanes(int viewInstanceID, NativeArray<Plane> planes)
	{
		m_SilhouettePlaneCache.Update(viewInstanceID, planes, m_FrameIndex);
	}

	internal OcclusionCullingDebugOutput GetOcclusionTestDebugOutput(int viewInstanceID)
	{
		if (m_ViewIDToIndexMap.TryGetValue(viewInstanceID, out var item) && m_OccluderContextSlots[item].valid)
		{
			return m_OccluderContextData[item].GetDebugOutput();
		}
		return default(OcclusionCullingDebugOutput);
	}

	public void UpdateOccluderStats(DebugRendererBatcherStats debugStats)
	{
		debugStats.occluderStats.Clear();
		foreach (KeyValue<int, int> item in m_ViewIDToIndexMap)
		{
			if (item.Value < m_OccluderContextSlots.Length && m_OccluderContextSlots[item.Value].valid)
			{
				ref NativeList<DebugOccluderStats> occluderStats = ref debugStats.occluderStats;
				DebugOccluderStats value = new DebugOccluderStats
				{
					viewInstanceID = item.Key,
					subviewCount = m_OccluderContextData[item.Value].subviewCount,
					occluderMipLayoutSize = m_OccluderContextData[item.Value].occluderMipLayoutSize
				};
				occluderStats.Add(in value);
			}
		}
	}

	internal bool HasOccluderContext(int viewInstanceID)
	{
		return m_ViewIDToIndexMap.ContainsKey(viewInstanceID);
	}

	internal bool GetOccluderContext(int viewInstanceID, out OccluderContext occluderContext)
	{
		if (m_ViewIDToIndexMap.TryGetValue(viewInstanceID, out var item) && m_OccluderContextSlots[item].valid)
		{
			occluderContext = m_OccluderContextData[item];
			return true;
		}
		occluderContext = default(OccluderContext);
		return false;
	}

	internal void UpdateFrame()
	{
		for (int i = 0; i < m_OccluderContextData.Count; i++)
		{
			if (m_OccluderContextSlots[i].valid)
			{
				OccluderContext value = m_OccluderContextData[i];
				OccluderContextSlot occluderContextSlot = m_OccluderContextSlots[i];
				if (m_FrameIndex - occluderContextSlot.lastUsedFrameIndex >= s_MaxContextGCFrame)
				{
					DeleteContext(occluderContextSlot.viewInstanceID);
					continue;
				}
				value.debugNeedsClear = true;
				m_OccluderContextData[i] = value;
			}
		}
		m_SilhouettePlaneCache.FreeUnusedSlots(m_FrameIndex, s_MaxContextGCFrame);
		m_FrameIndex++;
	}

	private int NewContext(int viewInstanceID)
	{
		int num = -1;
		OccluderContextSlot occluderContextSlot = default(OccluderContextSlot);
		occluderContextSlot.valid = true;
		occluderContextSlot.viewInstanceID = viewInstanceID;
		occluderContextSlot.lastUsedFrameIndex = m_FrameIndex;
		OccluderContextSlot value = occluderContextSlot;
		OccluderContext occluderContext = default(OccluderContext);
		if (m_FreeOccluderContexts.Length > 0)
		{
			num = m_FreeOccluderContexts[m_FreeOccluderContexts.Length - 1];
			m_FreeOccluderContexts.RemoveAt(m_FreeOccluderContexts.Length - 1);
			m_OccluderContextData[num] = occluderContext;
			m_OccluderContextSlots[num] = value;
		}
		else
		{
			num = m_OccluderContextData.Count;
			m_OccluderContextData.Add(occluderContext);
			m_OccluderContextSlots.Add(in value);
		}
		m_ViewIDToIndexMap.Add(viewInstanceID, num);
		return num;
	}

	private void DeleteContext(int viewInstanceID)
	{
		if (m_ViewIDToIndexMap.TryGetValue(viewInstanceID, out var item) && m_OccluderContextSlots[item].valid)
		{
			m_OccluderContextData[item].Dispose();
			m_OccluderContextSlots[item] = new OccluderContextSlot
			{
				valid = false
			};
			m_FreeOccluderContexts.Add(in item);
			m_ViewIDToIndexMap.Remove(viewInstanceID);
		}
	}

	public void Dispose()
	{
		CoreUtils.Destroy(m_DebugOcclusionTestMaterial);
		CoreUtils.Destroy(m_OccluderDebugViewMaterial);
		for (int i = 0; i < m_OccluderContextData.Count; i++)
		{
			if (m_OccluderContextSlots[i].valid)
			{
				m_OccluderContextData[i].Dispose();
			}
		}
		m_SilhouettePlaneCache.Dispose();
		m_ViewIDToIndexMap.Dispose();
		m_FreeOccluderContexts.Dispose();
		m_OccluderContextData.Clear();
		m_OccluderContextSlots.Dispose();
		m_CommonShaderVariables.Dispose();
		m_CommonConstantBuffer.Release();
		m_DebugShaderVariables.Dispose();
		m_DebugConstantBuffer.Release();
	}
}
