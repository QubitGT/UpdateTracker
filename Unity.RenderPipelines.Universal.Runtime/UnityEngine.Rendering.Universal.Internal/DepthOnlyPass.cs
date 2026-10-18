using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

public class DepthOnlyPass : ScriptableRenderPass
{
	private class PassData
	{
		internal RendererListHandle rendererList;
	}

	private GraphicsFormat depthStencilFormat;

	private PassData m_PassData;

	private FilteringSettings m_FilteringSettings;

	private static readonly ShaderTagId k_ShaderTagId = new ShaderTagId("DepthOnly");

	private static readonly int s_CameraDepthTextureID = Shader.PropertyToID("_CameraDepthTexture");

	private RTHandle destination { get; set; }

	internal ShaderTagId shaderTagId { get; set; } = k_ShaderTagId;


	public DepthOnlyPass(RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask)
	{
		base.profilingSampler = new ProfilingSampler("Draw Depth Only");
		m_PassData = new PassData();
		m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
		base.renderPassEvent = evt;
		base.useNativeRenderPass = false;
		shaderTagId = k_ShaderTagId;
	}

	public void Setup(RenderTextureDescriptor baseDescriptor, RTHandle depthAttachmentHandle)
	{
		destination = depthAttachmentHandle;
		depthStencilFormat = baseDescriptor.depthStencilFormat;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		_ = ref renderingData.cameraData.cameraTargetDescriptor;
		if (renderingData.cameraData.renderer.useDepthPriming && (renderingData.cameraData.renderType == CameraRenderType.Base || renderingData.cameraData.clearDepth))
		{
			ConfigureTarget(renderingData.cameraData.renderer.cameraDepthTargetHandle);
			ConfigureClear(ClearFlag.Depth, Color.black);
		}
		else
		{
			base.useNativeRenderPass = true;
			ConfigureTarget(destination);
			ConfigureClear(ClearFlag.All, Color.black);
		}
	}

	private static void ExecutePass(RasterCommandBuffer cmd, RendererList rendererList)
	{
		using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.DepthPrepass)))
		{
			cmd.DrawRendererList(rendererList);
		}
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		ContextContainer frameData = renderingData.frameData;
		UniversalRenderingData renderingData2 = frameData.Get<UniversalRenderingData>();
		UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		RendererListParams param = InitRendererListParams(renderingData2, cameraData, lightData);
		RendererList rendererList = context.CreateRendererList(ref param);
		ExecutePass(CommandBufferHelpers.GetRasterCommandBuffer(renderingData.commandBuffer), rendererList);
	}

	private RendererListParams InitRendererListParams(UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData)
	{
		SortingCriteria defaultOpaqueSortFlags = cameraData.defaultOpaqueSortFlags;
		DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(shaderTagId, renderingData, cameraData, lightData, defaultOpaqueSortFlags);
		drawSettings.perObjectData = PerObjectData.None;
		drawSettings.lodCrossFadeStencilMask = 0;
		return new RendererListParams(renderingData.cullResults, drawSettings, m_FilteringSettings);
	}

	internal void Render(RenderGraph renderGraph, ContextContainer frameData, ref TextureHandle cameraDepthTexture, uint batchLayerMask, bool setGlobalDepth)
	{
		UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\DepthOnlyPass.cs", 131);
		RendererListParams desc = InitRendererListParams(renderingData, universalCameraData, lightData);
		desc.filteringSettings.batchLayerMask = batchLayerMask;
		passData.rendererList = renderGraph.CreateRendererList(in desc);
		rasterRenderGraphBuilder.UseRendererList(in passData.rendererList);
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(cameraDepthTexture);
		if (setGlobalDepth)
		{
			rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in cameraDepthTexture, s_CameraDepthTextureID);
		}
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		if (universalCameraData.xr.enabled)
		{
			rasterRenderGraphBuilder.EnableFoveatedRasterization(universalCameraData.xr.supportsFoveatedRendering && universalCameraData.xrUniversal.canFoveateIntermediatePasses);
		}
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			ExecutePass(context.cmd, data.rendererList);
		});
	}
}
