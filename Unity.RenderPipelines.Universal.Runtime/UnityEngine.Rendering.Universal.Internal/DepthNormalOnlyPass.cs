using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

public class DepthNormalOnlyPass : ScriptableRenderPass
{
	private class PassData
	{
		internal TextureHandle cameraDepthTexture;

		internal TextureHandle cameraNormalsTexture;

		internal bool enableRenderingLayers;

		internal RenderingLayerUtils.MaskSize maskSize;

		internal RendererListHandle rendererList;
	}

	private FilteringSettings m_FilteringSettings;

	private PassData m_PassData;

	private static readonly List<ShaderTagId> k_DepthNormals = new List<ShaderTagId>
	{
		new ShaderTagId("DepthNormals"),
		new ShaderTagId("DepthNormalsOnly")
	};

	private static readonly RTHandle[] k_ColorAttachment1 = new RTHandle[1];

	private static readonly RTHandle[] k_ColorAttachment2 = new RTHandle[2];

	internal static readonly string k_CameraNormalsTextureName = "_CameraNormalsTexture";

	private static readonly int s_CameraDepthTextureID = Shader.PropertyToID("_CameraDepthTexture");

	private static readonly int s_CameraNormalsTextureID = Shader.PropertyToID(k_CameraNormalsTextureName);

	private static readonly int s_CameraRenderingLayersTextureID = Shader.PropertyToID("_CameraRenderingLayersTexture");

	internal List<ShaderTagId> shaderTagIds { get; set; }

	private RTHandle depthHandle { get; set; }

	private RTHandle normalHandle { get; set; }

	private RTHandle renderingLayersHandle { get; set; }

	internal bool enableRenderingLayers { get; set; }

	internal RenderingLayerUtils.MaskSize renderingLayersMaskSize { get; set; }

	public DepthNormalOnlyPass(RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask)
	{
		base.profilingSampler = ProfilingSampler.Get(URPProfileId.DrawDepthNormalPrepass);
		m_PassData = new PassData();
		m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
		base.renderPassEvent = evt;
		base.useNativeRenderPass = false;
		shaderTagIds = k_DepthNormals;
	}

	public static GraphicsFormat GetGraphicsFormat()
	{
		if (SystemInfo.IsFormatSupported(GraphicsFormat.R8G8B8A8_SNorm, GraphicsFormatUsage.Render))
		{
			return GraphicsFormat.R8G8B8A8_SNorm;
		}
		if (SystemInfo.IsFormatSupported(GraphicsFormat.R16G16B16A16_SFloat, GraphicsFormatUsage.Render))
		{
			return GraphicsFormat.R16G16B16A16_SFloat;
		}
		return GraphicsFormat.R32G32B32A32_SFloat;
	}

	public void Setup(RTHandle depthHandle, RTHandle normalHandle)
	{
		this.depthHandle = depthHandle;
		this.normalHandle = normalHandle;
		enableRenderingLayers = false;
	}

	public void Setup(RTHandle depthHandle, RTHandle normalHandle, RTHandle decalLayerHandle)
	{
		Setup(depthHandle, normalHandle);
		renderingLayersHandle = decalLayerHandle;
		enableRenderingLayers = true;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		RTHandle[] array;
		if (enableRenderingLayers)
		{
			k_ColorAttachment2[0] = normalHandle;
			k_ColorAttachment2[1] = renderingLayersHandle;
			array = k_ColorAttachment2;
		}
		else
		{
			k_ColorAttachment1[0] = normalHandle;
			array = k_ColorAttachment1;
		}
		if (renderingData.cameraData.renderer.useDepthPriming && (renderingData.cameraData.renderType == CameraRenderType.Base || renderingData.cameraData.clearDepth))
		{
			ConfigureTarget(array, renderingData.cameraData.renderer.cameraDepthTargetHandle);
		}
		else
		{
			ConfigureTarget(array, depthHandle);
		}
		ConfigureClear(ClearFlag.All, Color.black);
	}

	private static void ExecutePass(RasterCommandBuffer cmd, PassData passData, RendererList rendererList)
	{
		if (passData.enableRenderingLayers)
		{
			cmd.SetKeyword(in ShaderGlobalKeywords.WriteRenderingLayers, value: true);
		}
		cmd.DrawRendererList(rendererList);
		if (passData.enableRenderingLayers)
		{
			cmd.SetKeyword(in ShaderGlobalKeywords.WriteRenderingLayers, value: false);
		}
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		ContextContainer frameData = renderingData.frameData;
		UniversalRenderingData renderingData2 = frameData.Get<UniversalRenderingData>();
		UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		m_PassData.enableRenderingLayers = enableRenderingLayers;
		RendererListParams param = InitRendererListParams(renderingData2, cameraData, lightData);
		RendererList rendererList = context.CreateRendererList(ref param);
		RasterCommandBuffer rasterCommandBuffer = CommandBufferHelpers.GetRasterCommandBuffer(renderingData.commandBuffer);
		using (new ProfilingScope(rasterCommandBuffer, base.profilingSampler))
		{
			ExecutePass(rasterCommandBuffer, m_PassData, rendererList);
		}
	}

	public override void OnCameraCleanup(CommandBuffer cmd)
	{
		if (cmd == null)
		{
			throw new ArgumentNullException("cmd");
		}
		normalHandle = null;
		depthHandle = null;
		renderingLayersHandle = null;
		shaderTagIds = k_DepthNormals;
	}

	private RendererListParams InitRendererListParams(UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData)
	{
		SortingCriteria defaultOpaqueSortFlags = cameraData.defaultOpaqueSortFlags;
		DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(shaderTagIds, renderingData, cameraData, lightData, defaultOpaqueSortFlags);
		drawSettings.perObjectData = PerObjectData.None;
		return new RendererListParams(renderingData.cullResults, drawSettings, m_FilteringSettings);
	}

	internal void Render(RenderGraph renderGraph, ContextContainer frameData, TextureHandle cameraNormalsTexture, TextureHandle cameraDepthTexture, TextureHandle renderingLayersTexture, uint batchLayerMask, bool setGlobalDepth, bool setGlobalTextures)
	{
		UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\DepthNormalOnlyPass.cs", 196);
		passData.cameraNormalsTexture = cameraNormalsTexture;
		rasterRenderGraphBuilder.SetRenderAttachment(cameraNormalsTexture, 0);
		passData.cameraDepthTexture = cameraDepthTexture;
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(cameraDepthTexture);
		passData.enableRenderingLayers = enableRenderingLayers;
		if (passData.enableRenderingLayers)
		{
			rasterRenderGraphBuilder.SetRenderAttachment(renderingLayersTexture, 1);
			passData.maskSize = renderingLayersMaskSize;
		}
		RendererListParams desc = InitRendererListParams(renderingData, universalCameraData, lightData);
		desc.filteringSettings.batchLayerMask = batchLayerMask;
		passData.rendererList = renderGraph.CreateRendererList(in desc);
		rasterRenderGraphBuilder.UseRendererList(in passData.rendererList);
		if (universalCameraData.xr.enabled)
		{
			rasterRenderGraphBuilder.EnableFoveatedRasterization(universalCameraData.xr.supportsFoveatedRendering && universalCameraData.xrUniversal.canFoveateIntermediatePasses);
		}
		if (setGlobalTextures)
		{
			rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in cameraNormalsTexture, s_CameraNormalsTextureID);
			if (passData.enableRenderingLayers)
			{
				rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in renderingLayersTexture, s_CameraRenderingLayersTextureID);
			}
		}
		if (setGlobalDepth)
		{
			rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in cameraDepthTexture, s_CameraDepthTextureID);
		}
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			RenderingLayerUtils.SetupProperties(context.cmd, data.maskSize);
			ExecutePass(context.cmd, data, data.rendererList);
		});
	}
}
