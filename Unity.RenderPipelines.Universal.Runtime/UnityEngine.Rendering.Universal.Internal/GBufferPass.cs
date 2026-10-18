using System;
using Unity.Collections;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

internal class GBufferPass : ScriptableRenderPass
{
	private class PassData
	{
		internal TextureHandle[] gbuffer;

		internal TextureHandle depth;

		internal DeferredLights deferredLights;

		internal RendererListHandle rendererListHdl;

		internal RendererListHandle objectsWithErrorRendererListHdl;

		internal RendererList rendererList;

		internal RendererList objectsWithErrorRendererList;
	}

	private static readonly int s_CameraNormalsTextureID = Shader.PropertyToID("_CameraNormalsTexture");

	private static readonly int s_CameraRenderingLayersTextureID = Shader.PropertyToID("_CameraRenderingLayersTexture");

	private static readonly ShaderTagId s_ShaderTagLit = new ShaderTagId("Lit");

	private static readonly ShaderTagId s_ShaderTagSimpleLit = new ShaderTagId("SimpleLit");

	private static readonly ShaderTagId s_ShaderTagUnlit = new ShaderTagId("Unlit");

	private static readonly ShaderTagId s_ShaderTagComplexLit = new ShaderTagId("ComplexLit");

	private static readonly ShaderTagId s_ShaderTagUniversalGBuffer = new ShaderTagId("UniversalGBuffer");

	private static readonly ShaderTagId s_ShaderTagUniversalMaterialType = new ShaderTagId("UniversalMaterialType");

	private DeferredLights m_DeferredLights;

	private static ShaderTagId[] s_ShaderTagValues;

	private static RenderStateBlock[] s_RenderStateBlocks;

	private FilteringSettings m_FilteringSettings;

	private RenderStateBlock m_RenderStateBlock;

	private PassData m_PassData;

	public GBufferPass(RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference, DeferredLights deferredLights)
	{
		base.profilingSampler = new ProfilingSampler("Draw GBuffer");
		base.renderPassEvent = evt;
		m_PassData = new PassData();
		m_DeferredLights = deferredLights;
		m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
		m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
		m_RenderStateBlock.stencilState = stencilState;
		m_RenderStateBlock.stencilReference = stencilReference;
		m_RenderStateBlock.mask = RenderStateMask.Stencil;
		if (s_ShaderTagValues == null)
		{
			s_ShaderTagValues = new ShaderTagId[5];
			s_ShaderTagValues[0] = s_ShaderTagLit;
			s_ShaderTagValues[1] = s_ShaderTagSimpleLit;
			s_ShaderTagValues[2] = s_ShaderTagUnlit;
			s_ShaderTagValues[3] = s_ShaderTagComplexLit;
			s_ShaderTagValues[4] = default(ShaderTagId);
		}
		if (s_RenderStateBlocks == null)
		{
			s_RenderStateBlocks = new RenderStateBlock[5];
			s_RenderStateBlocks[0] = DeferredLights.OverwriteStencil(m_RenderStateBlock, 96, 32);
			s_RenderStateBlocks[1] = DeferredLights.OverwriteStencil(m_RenderStateBlock, 96, 64);
			s_RenderStateBlocks[2] = DeferredLights.OverwriteStencil(m_RenderStateBlock, 96, 0);
			s_RenderStateBlocks[3] = DeferredLights.OverwriteStencil(m_RenderStateBlock, 96, 0);
			s_RenderStateBlocks[4] = s_RenderStateBlocks[0];
		}
	}

	public void Dispose()
	{
		m_DeferredLights?.ReleaseGbufferResources();
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
		RTHandle[] gbufferAttachments = m_DeferredLights.GbufferAttachments;
		if (cmd != null)
		{
			bool flag = true;
			if (m_DeferredLights.UseFramebufferFetch && m_DeferredLights.DepthCopyTexture != null && m_DeferredLights.DepthCopyTexture.rt != null)
			{
				m_DeferredLights.GbufferAttachments[m_DeferredLights.GbufferDepthIndex] = m_DeferredLights.DepthCopyTexture;
				flag = false;
			}
			for (int i = 0; i < gbufferAttachments.Length; i++)
			{
				if (i != m_DeferredLights.GBufferLightingIndex && (i != m_DeferredLights.GBufferNormalSmoothnessIndex || !m_DeferredLights.HasNormalPrepass) && (i != m_DeferredLights.GbufferDepthIndex || flag) && (!m_DeferredLights.UseFramebufferFetch || i == m_DeferredLights.GbufferDepthIndex || m_DeferredLights.HasDepthPrepass))
				{
					m_DeferredLights.ReAllocateGBufferIfNeeded(cameraTextureDescriptor, i);
					cmd.SetGlobalTexture(m_DeferredLights.GbufferAttachments[i].name, m_DeferredLights.GbufferAttachments[i].nameID);
				}
			}
		}
		if (m_DeferredLights.UseFramebufferFetch)
		{
			m_DeferredLights.UpdateDeferredInputAttachments();
		}
		ConfigureTarget(m_DeferredLights.GbufferAttachments, m_DeferredLights.DepthAttachment, m_DeferredLights.GbufferFormats);
		ConfigureClear(ClearFlag.None, Color.black);
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		ContextContainer frameData = renderingData.frameData;
		UniversalRenderingData renderingData2 = frameData.Get<UniversalRenderingData>();
		UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		m_PassData.deferredLights = m_DeferredLights;
		InitRendererLists(ref m_PassData, context, null, renderingData2, cameraData, lightData, useRenderGraph: false);
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		using (new ProfilingScope(commandBuffer, base.profilingSampler))
		{
			ExecutePass(CommandBufferHelpers.GetRasterCommandBuffer(commandBuffer), m_PassData, m_PassData.rendererList, m_PassData.objectsWithErrorRendererList);
			if (!m_DeferredLights.UseFramebufferFetch)
			{
				renderingData.commandBuffer.SetGlobalTexture(s_CameraNormalsTextureID, m_DeferredLights.GbufferAttachments[m_DeferredLights.GBufferNormalSmoothnessIndex]);
			}
		}
	}

	private static void ExecutePass(RasterCommandBuffer cmd, PassData data, RendererList rendererList, RendererList errorRendererList)
	{
		int num;
		if (data.deferredLights.UseRenderingLayers)
		{
			num = ((!data.deferredLights.HasRenderingLayerPrepass) ? 1 : 0);
			if (num != 0)
			{
				cmd.SetKeyword(in ShaderGlobalKeywords.WriteRenderingLayers, value: true);
			}
		}
		else
		{
			num = 0;
		}
		cmd.DrawRendererList(rendererList);
		if (num != 0)
		{
			cmd.SetKeyword(in ShaderGlobalKeywords.WriteRenderingLayers, value: false);
		}
	}

	private void InitRendererLists(ref PassData passData, ScriptableRenderContext context, RenderGraph renderGraph, UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData, bool useRenderGraph, uint batchLayerMask = uint.MaxValue)
	{
		ShaderTagId shaderTagId = s_ShaderTagUniversalGBuffer;
		DrawingSettings drawSettings = CreateDrawingSettings(shaderTagId, renderingData, cameraData, lightData, cameraData.defaultOpaqueSortFlags);
		FilteringSettings filteringSettings = m_FilteringSettings;
		filteringSettings.batchLayerMask = batchLayerMask;
		NativeArray<ShaderTagId> value = new NativeArray<ShaderTagId>(s_ShaderTagValues, Allocator.Temp);
		NativeArray<RenderStateBlock> value2 = new NativeArray<RenderStateBlock>(s_RenderStateBlocks, Allocator.Temp);
		RendererListParams rendererListParams = new RendererListParams(renderingData.cullResults, drawSettings, filteringSettings);
		rendererListParams.tagValues = value;
		rendererListParams.stateBlocks = value2;
		rendererListParams.tagName = s_ShaderTagUniversalMaterialType;
		rendererListParams.isPassTagName = false;
		RendererListParams param = rendererListParams;
		if (useRenderGraph)
		{
			passData.rendererListHdl = renderGraph.CreateRendererList(in param);
		}
		else
		{
			passData.rendererList = context.CreateRendererList(ref param);
		}
		value.Dispose();
		value2.Dispose();
	}

	internal void Render(RenderGraph renderGraph, ContextContainer frameData, TextureHandle cameraColor, TextureHandle cameraDepth, bool setGlobalTextures, uint batchLayerMask = uint.MaxValue)
	{
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
		UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\GBufferPass.cs", 232);
		bool flag = m_DeferredLights.UseRenderingLayers && !m_DeferredLights.UseLightLayers;
		passData.gbuffer = m_DeferredLights.GbufferTextureHandles;
		for (int i = 0; i < m_DeferredLights.GBufferSliceCount; i++)
		{
			rasterRenderGraphBuilder.SetRenderAttachment(passData.gbuffer[i], i);
		}
		RenderGraphUtils.UseDBufferIfValid(rasterRenderGraphBuilder, universalResourceData);
		passData.depth = cameraDepth;
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(cameraDepth);
		passData.deferredLights = m_DeferredLights;
		InitRendererLists(ref passData, default(ScriptableRenderContext), renderGraph, renderingData, cameraData, lightData, useRenderGraph: true);
		rasterRenderGraphBuilder.UseRendererList(in passData.rendererListHdl);
		rasterRenderGraphBuilder.UseRendererList(in passData.objectsWithErrorRendererListHdl);
		if (setGlobalTextures)
		{
			TextureHandle cameraNormalsTexture = universalResourceData.cameraNormalsTexture;
			rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in cameraNormalsTexture, s_CameraNormalsTextureID);
			if (flag)
			{
				cameraNormalsTexture = universalResourceData.renderingLayersTexture;
				rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in cameraNormalsTexture, s_CameraRenderingLayersTextureID);
			}
		}
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			ExecutePass(context.cmd, data, data.rendererListHdl, data.objectsWithErrorRendererListHdl);
		});
	}
}
