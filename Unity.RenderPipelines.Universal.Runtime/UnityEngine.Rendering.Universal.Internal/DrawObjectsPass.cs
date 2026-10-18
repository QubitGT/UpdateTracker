using System;
using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

public class DrawObjectsPass : ScriptableRenderPass
{
	internal class PassData
	{
		internal TextureHandle albedoHdl;

		internal TextureHandle depthHdl;

		internal UniversalCameraData cameraData;

		internal bool isOpaque;

		internal bool shouldTransparentsReceiveShadows;

		internal uint batchLayerMask;

		internal bool isActiveTargetBackBuffer;

		internal RendererListHandle rendererListHdl;

		internal RendererListHandle objectsWithErrorRendererListHdl;

		internal DebugRendererLists debugRendererLists;

		internal RendererList rendererList;

		internal RendererList objectsWithErrorRendererList;
	}

	private FilteringSettings m_FilteringSettings;

	private RenderStateBlock m_RenderStateBlock;

	private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

	private bool m_IsOpaque;

	public bool m_IsActiveTargetBackBuffer;

	public bool m_ShouldTransparentsReceiveShadows;

	private PassData m_PassData;

	private static readonly int s_DrawObjectPassDataPropID = Shader.PropertyToID("_DrawObjectPassData");

	public DrawObjectsPass(string profilerTag, ShaderTagId[] shaderTagIds, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference)
	{
		Init(opaque, evt, renderQueueRange, layerMask, stencilState, stencilReference, shaderTagIds);
		base.profilingSampler = new ProfilingSampler(profilerTag);
	}

	public DrawObjectsPass(string profilerTag, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference)
		: this(profilerTag, null, opaque, evt, renderQueueRange, layerMask, stencilState, stencilReference)
	{
	}

	internal DrawObjectsPass(URPProfileId profileId, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference)
	{
		Init(opaque, evt, renderQueueRange, layerMask, stencilState, stencilReference);
		base.profilingSampler = ProfilingSampler.Get(profileId);
	}

	internal void Init(bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference, ShaderTagId[] shaderTagIds = null)
	{
		if (shaderTagIds == null)
		{
			shaderTagIds = new ShaderTagId[3]
			{
				new ShaderTagId("SRPDefaultUnlit"),
				new ShaderTagId("UniversalForward"),
				new ShaderTagId("UniversalForwardOnly")
			};
		}
		m_PassData = new PassData();
		ShaderTagId[] array = shaderTagIds;
		foreach (ShaderTagId item in array)
		{
			m_ShaderTagIdList.Add(item);
		}
		base.renderPassEvent = evt;
		m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
		m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
		m_IsOpaque = opaque;
		m_ShouldTransparentsReceiveShadows = false;
		m_IsActiveTargetBackBuffer = false;
		if (stencilState.enabled)
		{
			m_RenderStateBlock.stencilReference = stencilReference;
			m_RenderStateBlock.mask = RenderStateMask.Stencil;
			m_RenderStateBlock.stencilState = stencilState;
		}
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		ContextContainer frameData = renderingData.frameData;
		UniversalRenderingData renderingData2 = frameData.Get<UniversalRenderingData>();
		UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		InitPassData(cameraData, ref m_PassData, uint.MaxValue, m_IsActiveTargetBackBuffer);
		InitRendererLists(renderingData2, cameraData, lightData, ref m_PassData, context, null, useRenderGraph: false);
		using (new ProfilingScope(renderingData.commandBuffer, base.profilingSampler))
		{
			ExecutePass(CommandBufferHelpers.GetRasterCommandBuffer(renderingData.commandBuffer), m_PassData, m_PassData.rendererList, m_PassData.objectsWithErrorRendererList, m_PassData.cameraData.IsCameraProjectionMatrixFlipped());
		}
	}

	internal static void ExecutePass(RasterCommandBuffer cmd, PassData data, RendererList rendererList, RendererList objectsWithErrorRendererList, bool yFlip)
	{
		Vector4 value = new Vector4(0f, 0f, 0f, data.isOpaque ? 1f : 0f);
		cmd.SetGlobalVector(s_DrawObjectPassDataPropID, value);
		if (data.cameraData.xr.enabled && data.isActiveTargetBackBuffer)
		{
			cmd.SetViewport(data.cameraData.xr.GetViewport());
		}
		float num = (yFlip ? (-1f) : 1f);
		Vector4 value2 = ((num < 0f) ? new Vector4(num, 1f, -1f, 1f) : new Vector4(num, 0f, 1f, 1f));
		cmd.SetGlobalVector(ShaderPropertyId.scaleBiasRt, value2);
		float value3 = ((data.cameraData.cameraTargetDescriptor.msaaSamples > 1 && data.isOpaque) ? 1f : 0f);
		cmd.SetGlobalFloat(ShaderPropertyId.alphaToMaskAvailable, value3);
		if (ScriptableRenderPass.GetActiveDebugHandler(data.cameraData) != null)
		{
			data.debugRendererLists.DrawWithRendererList(cmd);
		}
		else
		{
			cmd.DrawRendererList(rendererList);
		}
	}

	internal void InitPassData(UniversalCameraData cameraData, ref PassData passData, uint batchLayerMask, bool isActiveTargetBackBuffer = false)
	{
		passData.cameraData = cameraData;
		passData.isOpaque = m_IsOpaque;
		passData.shouldTransparentsReceiveShadows = m_ShouldTransparentsReceiveShadows;
		passData.batchLayerMask = batchLayerMask;
		passData.isActiveTargetBackBuffer = isActiveTargetBackBuffer;
	}

	internal void InitRendererLists(UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData, ref PassData passData, ScriptableRenderContext context, RenderGraph renderGraph, bool useRenderGraph)
	{
		_ = cameraData.camera;
		SortingCriteria sortingCriteria = (m_IsOpaque ? cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent);
		if (cameraData.renderer.useDepthPriming && m_IsOpaque && (cameraData.renderType == CameraRenderType.Base || cameraData.clearDepth))
		{
			sortingCriteria = SortingCriteria.SortingLayer | SortingCriteria.RenderQueue | SortingCriteria.OptimizeStateChanges | SortingCriteria.CanvasOrder;
		}
		FilteringSettings filteringSettings = m_FilteringSettings;
		filteringSettings.batchLayerMask = passData.batchLayerMask;
		DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList, renderingData, cameraData, lightData, sortingCriteria);
		if (cameraData.renderer.useDepthPriming && m_IsOpaque && (cameraData.renderType == CameraRenderType.Base || cameraData.clearDepth))
		{
			m_RenderStateBlock.depthState = new DepthState(writeEnabled: false, CompareFunction.Equal);
			m_RenderStateBlock.mask |= RenderStateMask.Depth;
		}
		else if (m_RenderStateBlock.depthState.compareFunction == CompareFunction.Equal)
		{
			m_RenderStateBlock.depthState = new DepthState(writeEnabled: true, CompareFunction.LessEqual);
			m_RenderStateBlock.mask |= RenderStateMask.Depth;
		}
		DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(cameraData);
		if (useRenderGraph)
		{
			if (activeDebugHandler != null)
			{
				passData.debugRendererLists = activeDebugHandler.CreateRendererListsWithDebugRenderState(renderGraph, ref renderingData.cullResults, ref drawingSettings, ref filteringSettings, ref m_RenderStateBlock);
			}
			else
			{
				RenderingUtils.CreateRendererListWithRenderStateBlock(renderGraph, ref renderingData.cullResults, drawingSettings, filteringSettings, m_RenderStateBlock, ref passData.rendererListHdl);
			}
		}
		else if (activeDebugHandler != null)
		{
			passData.debugRendererLists = activeDebugHandler.CreateRendererListsWithDebugRenderState(context, ref renderingData.cullResults, ref drawingSettings, ref filteringSettings, ref m_RenderStateBlock);
		}
		else
		{
			RenderingUtils.CreateRendererListWithRenderStateBlock(context, ref renderingData.cullResults, drawingSettings, filteringSettings, m_RenderStateBlock, ref passData.rendererList);
		}
	}

	internal void Render(RenderGraph renderGraph, ContextContainer frameData, TextureHandle colorTarget, TextureHandle depthTarget, TextureHandle mainShadowsTexture, TextureHandle additionalShadowsTexture, uint batchLayerMask = uint.MaxValue)
	{
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\DrawObjectsPass.cs", 264);
		rasterRenderGraphBuilder.UseAllGlobalTextures(enable: true);
		InitPassData(universalCameraData, ref passData, batchLayerMask, universalResourceData.isActiveTargetBackBuffer);
		if (colorTarget.IsValid())
		{
			passData.albedoHdl = colorTarget;
			rasterRenderGraphBuilder.SetRenderAttachment(colorTarget, 0);
		}
		if (depthTarget.IsValid())
		{
			passData.depthHdl = depthTarget;
			rasterRenderGraphBuilder.SetRenderAttachmentDepth(depthTarget);
		}
		if (mainShadowsTexture.IsValid())
		{
			rasterRenderGraphBuilder.UseTexture(in mainShadowsTexture);
		}
		if (additionalShadowsTexture.IsValid())
		{
			rasterRenderGraphBuilder.UseTexture(in additionalShadowsTexture);
		}
		TextureHandle ssaoTexture = universalResourceData.ssaoTexture;
		if (ssaoTexture.IsValid())
		{
			rasterRenderGraphBuilder.UseTexture(in ssaoTexture);
		}
		RenderGraphUtils.UseDBufferIfValid(rasterRenderGraphBuilder, universalResourceData);
		InitRendererLists(renderingData, universalCameraData, lightData, ref passData, default(ScriptableRenderContext), renderGraph, useRenderGraph: true);
		if (ScriptableRenderPass.GetActiveDebugHandler(universalCameraData) != null)
		{
			passData.debugRendererLists.PrepareRendererListForRasterPass(rasterRenderGraphBuilder);
		}
		else
		{
			rasterRenderGraphBuilder.UseRendererList(in passData.rendererListHdl);
			rasterRenderGraphBuilder.UseRendererList(in passData.objectsWithErrorRendererListHdl);
		}
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		if (universalCameraData.xr.enabled)
		{
			bool flag = universalCameraData.xrUniversal.canFoveateIntermediatePasses || universalResourceData.isActiveTargetBackBuffer;
			rasterRenderGraphBuilder.EnableFoveatedRasterization(universalCameraData.xr.supportsFoveatedRendering && flag);
		}
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			if (!data.isOpaque && !data.shouldTransparentsReceiveShadows)
			{
				TransparentSettingsPass.ExecutePass(context.cmd);
			}
			bool yFlip = data.cameraData.IsRenderTargetProjectionMatrixFlipped(data.albedoHdl, data.depthHdl);
			ExecutePass(context.cmd, data, data.rendererListHdl, data.objectsWithErrorRendererListHdl, yFlip);
		});
	}
}
