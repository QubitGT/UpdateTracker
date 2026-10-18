using System;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class DrawNormal2DPass : ScriptableRenderPass
{
	private class PassData
	{
		internal RendererListHandle rendererList;
	}

	private static readonly string k_NormalPass = "Normal2D Pass";

	private static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler(k_NormalPass);

	private static readonly ShaderTagId k_NormalsRenderingPassName = new ShaderTagId("NormalsRendering");

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		throw new NotImplementedException();
	}

	private static void Execute(RasterCommandBuffer cmd, PassData passData)
	{
		cmd.DrawRendererList(passData.rendererList);
	}

	public void Render(RenderGraph graph, ContextContainer frameData, Renderer2DData rendererData, ref LayerBatch layerBatch, int batchIndex)
	{
		Universal2DResourceData universal2DResourceData = frameData.Get<Universal2DResourceData>();
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		if (!layerBatch.useNormals)
		{
			return;
		}
		UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = graph.AddRasterRenderPass<PassData>(k_NormalPass, out passData, m_ProfilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\2D\\Rendergraph\\DrawNormal2DPass.cs", 42);
		LayerUtility.GetFilterSettings(rendererData, ref layerBatch, out var filterSettings);
		DrawingSettings drawSettings = CreateDrawingSettings(k_NormalsRenderingPassName, universalRenderingData, universalCameraData, lightData, SortingCriteria.CommonTransparent);
		SortingSettings sortingSettings = drawSettings.sortingSettings;
		RendererLighting.GetTransparencySortingMode(rendererData, universalCameraData.camera, ref sortingSettings);
		drawSettings.sortingSettings = sortingSettings;
		rasterRenderGraphBuilder.AllowPassCulling(value: false);
		rasterRenderGraphBuilder.SetRenderAttachment(universal2DResourceData.normalsTexture[batchIndex], 0);
		if (rendererData.useDepthStencilBuffer)
		{
			TextureHandle tex = (universal2DResourceData.normalsDepth.IsValid() ? universal2DResourceData.normalsDepth : universalResourceData.activeDepthTexture);
			rasterRenderGraphBuilder.SetRenderAttachmentDepth(tex);
		}
		RendererListParams desc = new RendererListParams(universalRenderingData.cullResults, drawSettings, filterSettings);
		passData.rendererList = graph.CreateRendererList(in desc);
		rasterRenderGraphBuilder.UseRendererList(in passData.rendererList);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			Execute(context.cmd, data);
		});
	}
}
