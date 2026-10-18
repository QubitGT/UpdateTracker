using System;
using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class DecalForwardEmissivePass : ScriptableRenderPass
{
	private class PassData
	{
		internal DecalDrawFowardEmissiveSystem drawSystem;

		internal RendererListHandle rendererList;
	}

	private FilteringSettings m_FilteringSettings;

	private List<ShaderTagId> m_ShaderTagIdList;

	private DecalDrawFowardEmissiveSystem m_DrawSystem;

	private PassData m_PassData;

	public DecalForwardEmissivePass(DecalDrawFowardEmissiveSystem drawSystem)
	{
		base.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
		ConfigureInput(ScriptableRenderPassInput.Depth);
		m_DrawSystem = drawSystem;
		base.profilingSampler = new ProfilingSampler("Draw Decal Forward Emissive");
		m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque);
		m_ShaderTagIdList = new List<ShaderTagId>();
		m_ShaderTagIdList.Add(new ShaderTagId("DecalMeshForwardEmissive"));
		m_ShaderTagIdList.Add(new ShaderTagId("DecalProjectorForwardEmissive"));
		m_PassData = new PassData();
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		InitPassData(ref m_PassData);
		UniversalRenderingData universalRenderingData = renderingData.frameData.Get<UniversalRenderingData>();
		UniversalCameraData cameraData = renderingData.frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = renderingData.frameData.Get<UniversalLightData>();
		RendererListParams param = InitRendererListParams(universalRenderingData, cameraData, lightData);
		RendererList rendererList = context.CreateRendererList(ref param);
		using (new ProfilingScope(universalRenderingData.commandBuffer, base.profilingSampler))
		{
			ExecutePass(CommandBufferHelpers.GetRasterCommandBuffer(universalRenderingData.commandBuffer), m_PassData, rendererList);
		}
	}

	private void InitPassData(ref PassData passData)
	{
		passData.drawSystem = m_DrawSystem;
	}

	private RendererListParams InitRendererListParams(UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData)
	{
		SortingCriteria defaultOpaqueSortFlags = cameraData.defaultOpaqueSortFlags;
		DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList, renderingData, cameraData, lightData, defaultOpaqueSortFlags);
		return new RendererListParams(renderingData.cullResults, drawSettings, m_FilteringSettings);
	}

	private static void ExecutePass(RasterCommandBuffer cmd, PassData passData, RendererList rendererList)
	{
		passData.drawSystem.Execute(cmd);
		cmd.DrawRendererList(rendererList);
	}

	public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
	{
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Decal\\DBuffer\\DecalForwardEmissivePass.cs", 82);
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		InitPassData(ref passData);
		RendererListParams desc = InitRendererListParams(renderingData, universalCameraData, lightData);
		passData.rendererList = renderGraph.CreateRendererList(in desc);
		rasterRenderGraphBuilder.UseRendererList(in passData.rendererList);
		_ = (UniversalRenderer)universalCameraData.renderer;
		rasterRenderGraphBuilder.SetRenderAttachment(universalResourceData.activeColorTexture, 0);
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(universalResourceData.activeDepthTexture, AccessFlags.Read);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext rgContext)
		{
			ExecutePass(rgContext.cmd, data, data.rendererList);
		});
	}
}
