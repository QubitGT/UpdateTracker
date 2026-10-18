using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class DBufferRenderPass : ScriptableRenderPass
{
	private class PassData
	{
		internal DecalDrawDBufferSystem drawSystem;

		internal DBufferSettings settings;

		internal bool decalLayers;

		internal RTHandle dBufferDepth;

		internal RTHandle[] dBufferColorHandles;

		internal RendererListHandle rendererList;
	}

	internal static string[] s_DBufferNames = new string[4] { "_DBufferTexture0", "_DBufferTexture1", "_DBufferTexture2", "_DBufferTexture3" };

	internal static string s_DBufferDepthName = "DBufferDepth";

	private static readonly int s_SSAOTextureID = Shader.PropertyToID("_ScreenSpaceOcclusionTexture");

	private DecalDrawDBufferSystem m_DrawSystem;

	private DBufferSettings m_Settings;

	private Material m_DBufferClear;

	private FilteringSettings m_FilteringSettings;

	private List<ShaderTagId> m_ShaderTagIdList;

	private ProfilingSampler m_DBufferClearSampler;

	private bool m_DecalLayers;

	private RTHandle m_DBufferDepth;

	private PassData m_PassData;

	private TextureHandle[] dbufferHandles;

	internal RTHandle[] dBufferColorHandles { get; private set; }

	internal RTHandle depthHandle { get; private set; }

	internal RTHandle dBufferDepth => m_DBufferDepth;

	public DBufferRenderPass(Material dBufferClear, DBufferSettings settings, DecalDrawDBufferSystem drawSystem, bool decalLayers)
	{
		base.renderPassEvent = (RenderPassEvent)201;
		ScriptableRenderPassInput passInput = ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal;
		ConfigureInput(passInput);
		base.requiresIntermediateTexture = true;
		m_DrawSystem = drawSystem;
		m_Settings = settings;
		m_DBufferClear = dBufferClear;
		base.profilingSampler = new ProfilingSampler("Draw DBuffer");
		m_DBufferClearSampler = new ProfilingSampler("Clear");
		m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque);
		m_DecalLayers = decalLayers;
		m_ShaderTagIdList = new List<ShaderTagId>();
		m_ShaderTagIdList.Add(new ShaderTagId("DBufferMesh"));
		m_ShaderTagIdList.Add(new ShaderTagId("DBufferProjectorVFX"));
		int num = (int)(settings.surfaceData + 1);
		dBufferColorHandles = new RTHandle[num];
		m_PassData = new PassData();
	}

	public void Dispose()
	{
		m_DBufferDepth?.Release();
		RTHandle[] array = dBufferColorHandles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.Release();
		}
	}

	public void Setup(in CameraData cameraData)
	{
		RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
		descriptor.graphicsFormat = GraphicsFormat.None;
		descriptor.depthStencilFormat = cameraData.cameraTargetDescriptor.depthStencilFormat;
		descriptor.msaaSamples = 1;
		RenderingUtils.ReAllocateHandleIfNeeded(ref m_DBufferDepth, in descriptor, FilterMode.Point, TextureWrapMode.Repeat, 1, 0f, s_DBufferDepthName);
		Setup(in cameraData, m_DBufferDepth);
	}

	public void Setup(in CameraData cameraData, RTHandle depthTextureHandle)
	{
		RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
		descriptor.graphicsFormat = ((QualitySettings.activeColorSpace == ColorSpace.Linear) ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm);
		descriptor.depthStencilFormat = GraphicsFormat.None;
		descriptor.msaaSamples = 1;
		RenderingUtils.ReAllocateHandleIfNeeded(ref dBufferColorHandles[0], in descriptor, FilterMode.Point, TextureWrapMode.Repeat, 1, 0f, s_DBufferNames[0]);
		if (m_Settings.surfaceData == DecalSurfaceData.AlbedoNormal || m_Settings.surfaceData == DecalSurfaceData.AlbedoNormalMAOS)
		{
			RenderTextureDescriptor descriptor2 = cameraData.cameraTargetDescriptor;
			descriptor2.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
			descriptor2.depthStencilFormat = GraphicsFormat.None;
			descriptor2.msaaSamples = 1;
			RenderingUtils.ReAllocateHandleIfNeeded(ref dBufferColorHandles[1], in descriptor2, FilterMode.Point, TextureWrapMode.Repeat, 1, 0f, s_DBufferNames[1]);
		}
		if (m_Settings.surfaceData == DecalSurfaceData.AlbedoNormalMAOS)
		{
			RenderTextureDescriptor descriptor3 = cameraData.cameraTargetDescriptor;
			descriptor3.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
			descriptor3.depthStencilFormat = GraphicsFormat.None;
			descriptor3.msaaSamples = 1;
			RenderingUtils.ReAllocateHandleIfNeeded(ref dBufferColorHandles[2], in descriptor3, FilterMode.Point, TextureWrapMode.Repeat, 1, 0f, s_DBufferNames[2]);
		}
		depthHandle = depthTextureHandle;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		ConfigureTarget(dBufferColorHandles, depthHandle);
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		InitPassData(ref m_PassData);
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		PassData passData = m_PassData;
		using (new ProfilingScope(commandBuffer, base.profilingSampler))
		{
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			SetGlobalTextures(renderingData.commandBuffer, m_PassData);
			SetKeywords(CommandBufferHelpers.GetRasterCommandBuffer(renderingData.commandBuffer), m_PassData);
			using (new ProfilingScope(commandBuffer, m_DBufferClearSampler))
			{
				Blitter.BlitTexture(commandBuffer, passData.dBufferColorHandles[0], new Vector4(1f, 1f, 0f, 0f), m_DBufferClear, 0);
			}
			UniversalRenderingData renderingData2 = renderingData.frameData.Get<UniversalRenderingData>();
			UniversalCameraData cameraData = renderingData.frameData.Get<UniversalCameraData>();
			UniversalLightData lightData = renderingData.frameData.Get<UniversalLightData>();
			RendererListParams param = InitRendererListParams(renderingData2, cameraData, lightData);
			RendererList rendererList = context.CreateRendererList(ref param);
			ExecutePass(CommandBufferHelpers.GetRasterCommandBuffer(renderingData.commandBuffer), m_PassData, rendererList, renderGraph: false);
		}
	}

	private static void ExecutePass(RasterCommandBuffer cmd, PassData passData, RendererList rendererList, bool renderGraph)
	{
		passData.drawSystem.Execute(cmd);
		cmd.DrawRendererList(rendererList);
	}

	private static void SetGlobalTextures(CommandBuffer cmd, PassData passData)
	{
		RTHandle[] array = passData.dBufferColorHandles;
		cmd.SetGlobalTexture(array[0].name, array[0].nameID);
		if (passData.settings.surfaceData == DecalSurfaceData.AlbedoNormal || passData.settings.surfaceData == DecalSurfaceData.AlbedoNormalMAOS)
		{
			cmd.SetGlobalTexture(array[1].name, array[1].nameID);
		}
		if (passData.settings.surfaceData == DecalSurfaceData.AlbedoNormalMAOS)
		{
			cmd.SetGlobalTexture(array[2].name, array[2].nameID);
		}
	}

	private static void SetKeywords(RasterCommandBuffer cmd, PassData passData)
	{
		cmd.SetKeyword(in ShaderGlobalKeywords.DBufferMRT1, passData.settings.surfaceData == DecalSurfaceData.Albedo);
		cmd.SetKeyword(in ShaderGlobalKeywords.DBufferMRT2, passData.settings.surfaceData == DecalSurfaceData.AlbedoNormal);
		cmd.SetKeyword(in ShaderGlobalKeywords.DBufferMRT3, passData.settings.surfaceData == DecalSurfaceData.AlbedoNormalMAOS);
		cmd.SetKeyword(in ShaderGlobalKeywords.DecalLayers, passData.decalLayers);
	}

	private void InitPassData(ref PassData passData)
	{
		passData.drawSystem = m_DrawSystem;
		passData.settings = m_Settings;
		passData.decalLayers = m_DecalLayers;
		passData.dBufferDepth = m_DBufferDepth;
		passData.dBufferColorHandles = dBufferColorHandles;
	}

	private RendererListParams InitRendererListParams(UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData)
	{
		SortingCriteria defaultOpaqueSortFlags = cameraData.defaultOpaqueSortFlags;
		DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList, renderingData, cameraData, lightData, defaultOpaqueSortFlags);
		return new RendererListParams(renderingData.cullResults, drawSettings, m_FilteringSettings);
	}

	public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
	{
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		TextureHandle cameraDepthTexture = universalResourceData.cameraDepthTexture;
		TextureHandle cameraNormalsTexture = universalResourceData.cameraNormalsTexture;
		TextureHandle tex = (universalResourceData.dBufferDepth.IsValid() ? universalResourceData.dBufferDepth : universalResourceData.activeDepthTexture);
		TextureHandle renderingLayersTexture = universalResourceData.renderingLayersTexture;
		PassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Decal\\DBuffer\\DBufferRenderPass.cs", 233))
		{
			InitPassData(ref passData);
			if (dbufferHandles == null)
			{
				dbufferHandles = new TextureHandle[3];
			}
			RenderTextureDescriptor cameraTargetDescriptor = universalCameraData.cameraTargetDescriptor;
			cameraTargetDescriptor.graphicsFormat = ((QualitySettings.activeColorSpace == ColorSpace.Linear) ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm);
			cameraTargetDescriptor.depthStencilFormat = GraphicsFormat.None;
			cameraTargetDescriptor.msaaSamples = 1;
			dbufferHandles[0] = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor, s_DBufferNames[0], clear: true, new Color(0f, 0f, 0f, 1f));
			rasterRenderGraphBuilder.SetRenderAttachment(dbufferHandles[0], 0);
			if (m_Settings.surfaceData == DecalSurfaceData.AlbedoNormal || m_Settings.surfaceData == DecalSurfaceData.AlbedoNormalMAOS)
			{
				RenderTextureDescriptor cameraTargetDescriptor2 = universalCameraData.cameraTargetDescriptor;
				cameraTargetDescriptor2.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
				cameraTargetDescriptor2.depthStencilFormat = GraphicsFormat.None;
				cameraTargetDescriptor2.msaaSamples = 1;
				dbufferHandles[1] = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor2, s_DBufferNames[1], clear: true, new Color(0.5f, 0.5f, 0.5f, 1f));
				rasterRenderGraphBuilder.SetRenderAttachment(dbufferHandles[1], 1);
			}
			if (m_Settings.surfaceData == DecalSurfaceData.AlbedoNormalMAOS)
			{
				RenderTextureDescriptor cameraTargetDescriptor3 = universalCameraData.cameraTargetDescriptor;
				cameraTargetDescriptor3.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
				cameraTargetDescriptor3.depthStencilFormat = GraphicsFormat.None;
				cameraTargetDescriptor3.msaaSamples = 1;
				dbufferHandles[2] = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor3, s_DBufferNames[2], clear: true, new Color(0f, 0f, 0f, 1f));
				rasterRenderGraphBuilder.SetRenderAttachment(dbufferHandles[2], 2);
			}
			rasterRenderGraphBuilder.SetRenderAttachmentDepth(tex, AccessFlags.Read);
			if (cameraDepthTexture.IsValid())
			{
				rasterRenderGraphBuilder.UseTexture(in cameraDepthTexture);
			}
			if (cameraNormalsTexture.IsValid())
			{
				rasterRenderGraphBuilder.UseTexture(in cameraNormalsTexture);
			}
			if (passData.decalLayers && renderingLayersTexture.IsValid())
			{
				rasterRenderGraphBuilder.UseTexture(in renderingLayersTexture);
			}
			if (universalResourceData.ssaoTexture.IsValid())
			{
				rasterRenderGraphBuilder.UseGlobalTexture(s_SSAOTextureID);
			}
			RendererListParams desc = InitRendererListParams(renderingData, universalCameraData, lightData);
			passData.rendererList = renderGraph.CreateRendererList(in desc);
			rasterRenderGraphBuilder.UseRendererList(in passData.rendererList);
			for (int i = 0; i < 3; i++)
			{
				if (dbufferHandles[i].IsValid())
				{
					rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in dbufferHandles[i], Shader.PropertyToID(s_DBufferNames[i]));
				}
			}
			rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
			rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext rgContext)
			{
				SetKeywords(rgContext.cmd, data);
				ExecutePass(rgContext.cmd, data, data.rendererList, renderGraph: true);
			});
		}
		universalResourceData.dBuffer = dbufferHandles;
	}

	public override void OnCameraCleanup(CommandBuffer cmd)
	{
		if (cmd == null)
		{
			throw new ArgumentNullException("cmd");
		}
		cmd.SetKeyword(in ShaderGlobalKeywords.DBufferMRT1, value: false);
		cmd.SetKeyword(in ShaderGlobalKeywords.DBufferMRT2, value: false);
		cmd.SetKeyword(in ShaderGlobalKeywords.DBufferMRT3, value: false);
		cmd.SetKeyword(in ShaderGlobalKeywords.DecalLayers, value: false);
	}
}
