using System;
using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class DrawRenderer2DPass : ScriptableRenderPass
{
	private class SetGlobalPassData
	{
		internal TextureHandle[] lightTextures;
	}

	private class PassData
	{
		internal Light2DBlendStyle[] lightBlendStyles;

		internal int[] blendStyleIndices;

		internal float hdrEmulationScale;

		internal bool isSceneLit;

		internal bool layerUseLights;

		internal TextureHandle[] lightTextures;

		internal RendererListHandle rendererList;

		internal DebugRendererLists debugRendererLists;

		internal bool activeDebugHandler;
	}

	private static readonly string k_RenderPass = "Renderer2D Pass";

	private static readonly string k_SetLightBlendTexture = "SetLightBlendTextures";

	private static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler(k_RenderPass);

	private static readonly ProfilingSampler m_SetLightBlendTextureProfilingSampler = new ProfilingSampler(k_SetLightBlendTexture);

	private static readonly ShaderTagId k_CombinedRenderingPassName = new ShaderTagId("Universal2D");

	private static readonly ShaderTagId k_LegacyPassName = new ShaderTagId("SRPDefaultUnlit");

	private static readonly List<ShaderTagId> k_ShaderTags = new List<ShaderTagId> { k_LegacyPassName, k_CombinedRenderingPassName };

	private static readonly int k_HDREmulationScaleID = Shader.PropertyToID("_HDREmulationScale");

	private static readonly int k_RendererColorID = Shader.PropertyToID("_RendererColor");

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		throw new NotImplementedException();
	}

	private static void Execute(RasterGraphContext context, PassData passData)
	{
		RasterCommandBuffer cmd = context.cmd;
		int num = passData.blendStyleIndices.Length;
		cmd.SetGlobalFloat(k_HDREmulationScaleID, passData.hdrEmulationScale);
		cmd.SetGlobalColor(k_RendererColorID, Color.white);
		RendererLighting.SetLightShaderGlobals(cmd, passData.lightBlendStyles, passData.blendStyleIndices);
		if (passData.layerUseLights)
		{
			for (int i = 0; i < num; i++)
			{
				int blendStyleIndex = passData.blendStyleIndices[i];
				RendererLighting.EnableBlendStyle(cmd, blendStyleIndex, enabled: true);
			}
		}
		else if (passData.isSceneLit)
		{
			RendererLighting.EnableBlendStyle(cmd, 0, enabled: true);
		}
		if (passData.activeDebugHandler)
		{
			passData.debugRendererLists.DrawWithRendererList(cmd);
		}
		else
		{
			cmd.DrawRendererList(passData.rendererList);
		}
		RendererLighting.DisableAllKeywords(cmd);
	}

	public void Render(RenderGraph graph, ContextContainer frameData, Renderer2DData rendererData, ref LayerBatch[] layerBatches, int batchIndex, ref FilteringSettings filterSettings)
	{
		UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		Universal2DResourceData universal2DResourceData = frameData.Get<Universal2DResourceData>();
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		LayerBatch layerBatch = layerBatches[batchIndex];
		bool isLitView = true;
		if (batchIndex == 0)
		{
			SetGlobalPassData passData;
			using IRasterRenderGraphBuilder rasterRenderGraphBuilder = graph.AddRasterRenderPass<SetGlobalPassData>(k_SetLightBlendTexture, out passData, m_SetLightBlendTextureProfilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\2D\\Rendergraph\\DrawRenderer2DPass.cs", 116);
			if (layerBatch.lightStats.useLights)
			{
				passData.lightTextures = universal2DResourceData.lightTextures[batchIndex];
				for (int i = 0; i < passData.lightTextures.Length; i++)
				{
					rasterRenderGraphBuilder.UseTexture(in passData.lightTextures[i]);
				}
			}
			SetGlobalLightTextures(graph, rasterRenderGraphBuilder, passData.lightTextures, ref layerBatch, rendererData, isLitView);
			rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
			rasterRenderGraphBuilder.SetRenderFunc<SetGlobalPassData>(delegate
			{
			});
		}
		PassData passData2;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder2 = graph.AddRasterRenderPass<PassData>(k_RenderPass, out passData2, m_ProfilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\2D\\Rendergraph\\DrawRenderer2DPass.cs", 136);
		passData2.lightBlendStyles = rendererData.lightBlendStyles;
		passData2.blendStyleIndices = layerBatch.activeBlendStylesIndices;
		passData2.hdrEmulationScale = rendererData.hdrEmulationScale;
		passData2.isSceneLit = rendererData.lightCullResult.IsSceneLit();
		passData2.layerUseLights = layerBatch.lightStats.useLights;
		DrawingSettings drawingSettings = CreateDrawingSettings(k_ShaderTags, universalRenderingData, universalCameraData, lightData, SortingCriteria.CommonTransparent);
		SortingSettings sortingSettings = drawingSettings.sortingSettings;
		RendererLighting.GetTransparencySortingMode(rendererData, universalCameraData.camera, ref sortingSettings);
		drawingSettings.sortingSettings = sortingSettings;
		DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(universalCameraData);
		passData2.activeDebugHandler = activeDebugHandler != null;
		if (activeDebugHandler != null)
		{
			RenderStateBlock renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
			passData2.debugRendererLists = activeDebugHandler.CreateRendererListsWithDebugRenderState(graph, ref universalRenderingData.cullResults, ref drawingSettings, ref filterSettings, ref renderStateBlock);
			passData2.debugRendererLists.PrepareRendererListForRasterPass(rasterRenderGraphBuilder2);
		}
		else
		{
			RendererListParams desc = new RendererListParams(universalRenderingData.cullResults, drawingSettings, filterSettings);
			passData2.rendererList = graph.CreateRendererList(in desc);
			rasterRenderGraphBuilder2.UseRendererList(in passData2.rendererList);
		}
		if (passData2.layerUseLights)
		{
			passData2.lightTextures = universal2DResourceData.lightTextures[batchIndex];
			for (int j = 0; j < passData2.lightTextures.Length; j++)
			{
				rasterRenderGraphBuilder2.UseTexture(in passData2.lightTextures[j]);
			}
		}
		if (rendererData.useCameraSortingLayerTexture)
		{
			TextureHandle cameraSortingLayerTexture = universal2DResourceData.cameraSortingLayerTexture;
			rasterRenderGraphBuilder2.UseTexture(in cameraSortingLayerTexture);
		}
		rasterRenderGraphBuilder2.SetRenderAttachment(universalResourceData.activeColorTexture, 0);
		if (rendererData.useDepthStencilBuffer)
		{
			rasterRenderGraphBuilder2.SetRenderAttachmentDepth(universalResourceData.activeDepthTexture);
		}
		rasterRenderGraphBuilder2.AllowGlobalStateModification(value: true);
		int num = batchIndex + 1;
		if (num < universal2DResourceData.lightTextures.Length)
		{
			SetGlobalLightTextures(graph, rasterRenderGraphBuilder2, universal2DResourceData.lightTextures[num], ref layerBatches[num], rendererData, isLitView);
		}
		rasterRenderGraphBuilder2.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			Execute(context, data);
		});
	}

	private void SetGlobalLightTextures(RenderGraph graph, IRasterRenderGraphBuilder builder, TextureHandle[] lightTextures, ref LayerBatch layerBatch, Renderer2DData rendererData, bool isLitView)
	{
		if (!isLitView)
		{
			return;
		}
		if (layerBatch.lightStats.useLights)
		{
			for (int i = 0; i < lightTextures.Length; i++)
			{
				int num = layerBatch.activeBlendStylesIndices[i];
				builder.SetGlobalTextureAfterPass(in lightTextures[i], Shader.PropertyToID(RendererLighting.k_ShapeLightTextureIDs[num]));
			}
		}
		else if (rendererData.lightCullResult.IsSceneLit())
		{
			for (int j = 0; j < RendererLighting.k_ShapeLightTextureIDs.Length; j++)
			{
				TextureHandle blackTexture = graph.defaultResources.blackTexture;
				builder.SetGlobalTextureAfterPass(in blackTexture, Shader.PropertyToID(RendererLighting.k_ShapeLightTextureIDs[j]));
			}
		}
	}
}
