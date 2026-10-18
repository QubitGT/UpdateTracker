using System;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

internal class DrawObjectsWithRenderingLayersPass : DrawObjectsPass
{
	private class RenderingLayersPassData
	{
		internal PassData basePassData;

		internal RenderingLayerUtils.MaskSize maskSize;

		public RenderingLayersPassData()
		{
			basePassData = new PassData();
		}
	}

	private RTHandle[] m_ColorTargetIndentifiers;

	private RTHandle m_DepthTargetIndentifiers;

	public DrawObjectsWithRenderingLayersPass(URPProfileId profilerTag, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference)
		: base(profilerTag, opaque, evt, renderQueueRange, layerMask, stencilState, stencilReference)
	{
		m_ColorTargetIndentifiers = new RTHandle[2];
	}

	public void Setup(RTHandle colorAttachment, RTHandle renderingLayersTexture, RTHandle depthAttachment)
	{
		if (colorAttachment == null)
		{
			throw new ArgumentException("Color attachment can not be null", "colorAttachment");
		}
		if (renderingLayersTexture == null)
		{
			throw new ArgumentException("Rendering layers attachment can not be null", "renderingLayersTexture");
		}
		if (depthAttachment == null)
		{
			throw new ArgumentException("Depth attachment can not be null", "depthAttachment");
		}
		m_ColorTargetIndentifiers[0] = colorAttachment;
		m_ColorTargetIndentifiers[1] = renderingLayersTexture;
		m_DepthTargetIndentifiers = depthAttachment;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
		ConfigureTarget(m_ColorTargetIndentifiers, m_DepthTargetIndentifiers);
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		commandBuffer.SetKeyword(in ShaderGlobalKeywords.WriteRenderingLayers, value: true);
		base.Execute(context, ref renderingData);
		commandBuffer.SetKeyword(in ShaderGlobalKeywords.WriteRenderingLayers, value: false);
	}

	internal void Render(RenderGraph renderGraph, ContextContainer frameData, TextureHandle colorTarget, TextureHandle renderingLayersTexture, TextureHandle depthTarget, TextureHandle mainShadowsTexture, TextureHandle additionalShadowsTexture, RenderingLayerUtils.MaskSize maskSize, uint batchLayerMask = uint.MaxValue)
	{
		RenderingLayersPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<RenderingLayersPassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\DrawObjectsPass.cs", 411);
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		InitPassData(universalCameraData, ref passData.basePassData, batchLayerMask);
		passData.maskSize = maskSize;
		passData.basePassData.albedoHdl = colorTarget;
		rasterRenderGraphBuilder.SetRenderAttachment(colorTarget, 0);
		rasterRenderGraphBuilder.SetRenderAttachment(renderingLayersTexture, 1);
		passData.basePassData.depthHdl = depthTarget;
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(depthTarget);
		if (mainShadowsTexture.IsValid())
		{
			rasterRenderGraphBuilder.UseTexture(in mainShadowsTexture);
		}
		if (additionalShadowsTexture.IsValid())
		{
			rasterRenderGraphBuilder.UseTexture(in additionalShadowsTexture);
		}
		if (universalCameraData.renderer is UniversalRenderer)
		{
			TextureHandle ssaoTexture = universalResourceData.ssaoTexture;
			if (ssaoTexture.IsValid())
			{
				rasterRenderGraphBuilder.UseTexture(in ssaoTexture);
			}
			RenderGraphUtils.UseDBufferIfValid(rasterRenderGraphBuilder, universalResourceData);
		}
		InitRendererLists(renderingData, universalCameraData, lightData, ref passData.basePassData, default(ScriptableRenderContext), renderGraph, useRenderGraph: true);
		if (ScriptableRenderPass.GetActiveDebugHandler(universalCameraData) != null)
		{
			passData.basePassData.debugRendererLists.PrepareRendererListForRasterPass(rasterRenderGraphBuilder);
		}
		else
		{
			rasterRenderGraphBuilder.UseRendererList(in passData.basePassData.rendererListHdl);
			rasterRenderGraphBuilder.UseRendererList(in passData.basePassData.objectsWithErrorRendererListHdl);
		}
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		if (universalCameraData.xr.enabled)
		{
			bool flag = universalCameraData.xrUniversal.canFoveateIntermediatePasses || universalResourceData.isActiveTargetBackBuffer;
			rasterRenderGraphBuilder.EnableFoveatedRasterization(universalCameraData.xr.supportsFoveatedRendering && flag);
		}
		rasterRenderGraphBuilder.SetRenderFunc(delegate(RenderingLayersPassData data, RasterGraphContext context)
		{
			context.cmd.SetKeyword(in ShaderGlobalKeywords.WriteRenderingLayers, value: true);
			RenderingLayerUtils.SetupProperties(context.cmd, data.maskSize);
			if (!data.basePassData.isOpaque && !data.basePassData.shouldTransparentsReceiveShadows)
			{
				TransparentSettingsPass.ExecutePass(context.cmd);
			}
			bool yFlip = data.basePassData.cameraData.IsRenderTargetProjectionMatrixFlipped(data.basePassData.albedoHdl, data.basePassData.depthHdl);
			DrawObjectsPass.ExecutePass(context.cmd, data.basePassData, data.basePassData.rendererListHdl, data.basePassData.objectsWithErrorRendererListHdl, yFlip);
			context.cmd.SetKeyword(in ShaderGlobalKeywords.WriteRenderingLayers, value: false);
		});
	}
}
