using System;
using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.Universal;

internal class DecalScreenSpaceRenderPass : ScriptableRenderPass
{
	private class PassData
	{
		internal DecalDrawScreenSpaceSystem drawSystem;

		internal DecalScreenSpaceSettings settings;

		internal bool decalLayers;

		internal bool isGLDevice;

		internal TextureHandle colorTarget;

		internal UniversalCameraData cameraData;

		internal RendererListHandle rendererList;
	}

	private FilteringSettings m_FilteringSettings;

	private List<ShaderTagId> m_ShaderTagIdList;

	private DecalDrawScreenSpaceSystem m_DrawSystem;

	private DecalScreenSpaceSettings m_Settings;

	private bool m_DecalLayers;

	private PassData m_PassData;

	public DecalScreenSpaceRenderPass(DecalScreenSpaceSettings settings, DecalDrawScreenSpaceSystem drawSystem, bool decalLayers)
	{
		base.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
		ScriptableRenderPassInput passInput = ScriptableRenderPassInput.Depth;
		ConfigureInput(passInput);
		m_DrawSystem = drawSystem;
		m_Settings = settings;
		base.profilingSampler = new ProfilingSampler("Draw Decal Screen Space");
		m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque);
		m_DecalLayers = decalLayers;
		m_ShaderTagIdList = new List<ShaderTagId>();
		if (m_DrawSystem == null)
		{
			m_ShaderTagIdList.Add(new ShaderTagId("DecalScreenSpaceProjector"));
		}
		else
		{
			m_ShaderTagIdList.Add(new ShaderTagId("DecalScreenSpaceMesh"));
		}
		m_PassData = new PassData();
	}

	private RendererListParams CreateRenderListParams(UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData)
	{
		SortingCriteria sortingCriteria = SortingCriteria.None;
		DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList, renderingData, cameraData, lightData, sortingCriteria);
		return new RendererListParams(renderingData.cullResults, drawSettings, m_FilteringSettings);
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		UniversalCameraData cameraData = renderingData.frameData.Get<UniversalCameraData>();
		InitPassData(cameraData, ref m_PassData);
		RenderingUtils.SetScaleBiasRt(CommandBufferHelpers.GetRasterCommandBuffer(renderingData.commandBuffer), in renderingData);
		UniversalRenderingData renderingData2 = renderingData.frameData.Get<UniversalRenderingData>();
		UniversalLightData lightData = renderingData.frameData.Get<UniversalLightData>();
		RendererListParams param = CreateRenderListParams(renderingData2, cameraData, lightData);
		RendererList rendererList = context.CreateRendererList(ref param);
		using (new ProfilingScope(renderingData.commandBuffer, base.profilingSampler))
		{
			ExecutePass(CommandBufferHelpers.GetRasterCommandBuffer(renderingData.commandBuffer), m_PassData, rendererList);
		}
	}

	private void InitPassData(UniversalCameraData cameraData, ref PassData passData)
	{
		passData.drawSystem = m_DrawSystem;
		passData.settings = m_Settings;
		passData.decalLayers = m_DecalLayers;
		passData.isGLDevice = DecalRendererFeature.isGLDevice;
		passData.cameraData = cameraData;
	}

	private static void ExecutePass(RasterCommandBuffer cmd, PassData passData, RendererList rendererList)
	{
		NormalReconstruction.SetupProperties(cmd, in passData.cameraData);
		cmd.SetKeyword(in ShaderGlobalKeywords.DecalNormalBlendLow, passData.settings.normalBlend == DecalNormalBlend.Low);
		cmd.SetKeyword(in ShaderGlobalKeywords.DecalNormalBlendMedium, passData.settings.normalBlend == DecalNormalBlend.Medium);
		cmd.SetKeyword(in ShaderGlobalKeywords.DecalNormalBlendHigh, passData.settings.normalBlend == DecalNormalBlend.High);
		if (!passData.isGLDevice)
		{
			cmd.SetKeyword(in ShaderGlobalKeywords.DecalLayers, passData.decalLayers);
		}
		passData.drawSystem?.Execute(cmd);
		cmd.DrawRendererList(rendererList);
	}

	public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
	{
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		TextureHandle cameraDepthTexture = universalResourceData.cameraDepthTexture;
		TextureHandle renderingLayersTexture = universalResourceData.renderingLayersTexture;
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Decal\\ScreenSpace\\DecalScreenSpaceRenderPass.cs", 114);
		UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
		UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		InitPassData(cameraData, ref passData);
		passData.colorTarget = universalResourceData.cameraColor;
		rasterRenderGraphBuilder.SetRenderAttachment(universalResourceData.activeColorTexture, 0);
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(universalResourceData.activeDepthTexture, AccessFlags.Read);
		RendererListParams desc = CreateRenderListParams(renderingData, passData.cameraData, lightData);
		passData.rendererList = renderGraph.CreateRendererList(in desc);
		rasterRenderGraphBuilder.UseRendererList(in passData.rendererList);
		if (cameraDepthTexture.IsValid())
		{
			rasterRenderGraphBuilder.UseTexture(in cameraDepthTexture);
		}
		if (passData.decalLayers && renderingLayersTexture.IsValid())
		{
			rasterRenderGraphBuilder.UseTexture(in renderingLayersTexture);
		}
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext rgContext)
		{
			RenderingUtils.SetScaleBiasRt(rgContext.cmd, in data.cameraData, data.colorTarget);
			ExecutePass(rgContext.cmd, data, data.rendererList);
		});
	}

	public override void OnCameraCleanup(CommandBuffer cmd)
	{
		if (cmd == null)
		{
			throw new ArgumentNullException("cmd");
		}
		cmd.SetKeyword(in ShaderGlobalKeywords.DecalNormalBlendLow, value: false);
		cmd.SetKeyword(in ShaderGlobalKeywords.DecalNormalBlendMedium, value: false);
		cmd.SetKeyword(in ShaderGlobalKeywords.DecalNormalBlendHigh, value: false);
		cmd.SetKeyword(in ShaderGlobalKeywords.DecalLayers, value: false);
	}
}
