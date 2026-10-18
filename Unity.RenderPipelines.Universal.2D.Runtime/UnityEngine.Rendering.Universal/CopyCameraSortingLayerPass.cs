using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class CopyCameraSortingLayerPass : ScriptableRenderPass
{
	private class PassData
	{
		internal TextureHandle source;
	}

	private static readonly string k_CopyCameraSortingLayerPass = "CopyCameraSortingLayer Pass";

	private static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler(k_CopyCameraSortingLayerPass);

	private static readonly ProfilingSampler m_ExecuteProfilingSampler = new ProfilingSampler("Copy");

	internal static readonly string k_CameraSortingLayerTexture = "_CameraSortingLayerTexture";

	internal static readonly int k_CameraSortingLayerTextureId = Shader.PropertyToID(k_CameraSortingLayerTexture);

	private static Material m_BlitMaterial;

	public CopyCameraSortingLayerPass(Material blitMaterial)
	{
		m_BlitMaterial = blitMaterial;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		throw new NotImplementedException();
	}

	public static void ConfigureDescriptor(Downsampling downsamplingMethod, ref RenderTextureDescriptor descriptor, out FilterMode filterMode)
	{
		descriptor.msaaSamples = 1;
		descriptor.depthStencilFormat = GraphicsFormat.None;
		switch (downsamplingMethod)
		{
		case Downsampling._2xBilinear:
			descriptor.width /= 2;
			descriptor.height /= 2;
			break;
		case Downsampling._4xBox:
		case Downsampling._4xBilinear:
			descriptor.width /= 4;
			descriptor.height /= 4;
			break;
		}
		filterMode = ((downsamplingMethod != 0 && downsamplingMethod != Downsampling._4xBox) ? FilterMode.Bilinear : FilterMode.Point);
	}

	private static void Execute(RasterCommandBuffer cmd, RTHandle source)
	{
		using (new ProfilingScope(cmd, m_ExecuteProfilingSampler))
		{
			Vector2 vector = (source.useScaling ? new Vector2(source.rtHandleProperties.rtHandleScale.x, source.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Blitter.BlitTexture(cmd, source, vector, m_BlitMaterial, (source.rt.filterMode == FilterMode.Bilinear) ? 1 : 0);
		}
	}

	public void Render(RenderGraph graph, ContextContainer frameData)
	{
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		Universal2DResourceData universal2DResourceData = frameData.Get<Universal2DResourceData>();
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = graph.AddRasterRenderPass<PassData>(k_CopyCameraSortingLayerPass, out passData, m_ProfilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\2D\\Rendergraph\\CopyCameraSortingLayerPass.cs", 65);
		passData.source = universalResourceData.activeColorTexture;
		rasterRenderGraphBuilder.SetRenderAttachment(universal2DResourceData.cameraSortingLayerTexture, 0);
		rasterRenderGraphBuilder.UseTexture(in passData.source);
		rasterRenderGraphBuilder.AllowPassCulling(value: false);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			Execute(context.cmd, data.source);
		});
	}
}
