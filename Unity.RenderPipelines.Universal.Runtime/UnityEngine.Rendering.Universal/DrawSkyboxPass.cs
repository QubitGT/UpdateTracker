using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

public class DrawSkyboxPass : ScriptableRenderPass
{
	private class PassData
	{
		internal XRPass xr;

		internal RendererListHandle skyRendererListHandle;

		internal Material material;
	}

	public DrawSkyboxPass(RenderPassEvent evt)
	{
		base.profilingSampler = ProfilingSampler.Get(URPProfileId.DrawSkybox);
		base.renderPassEvent = evt;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		UniversalCameraData universalCameraData = renderingData.frameData.Get<UniversalCameraData>();
		DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(universalCameraData);
		if (activeDebugHandler == null || !activeDebugHandler.IsScreenClearNeeded)
		{
			RendererList rendererList = CreateSkyboxRendererList(context, universalCameraData);
			ExecutePass(CommandBufferHelpers.GetRasterCommandBuffer(renderingData.commandBuffer), universalCameraData.xr, rendererList);
		}
	}

	private RendererList CreateSkyboxRendererList(ScriptableRenderContext context, UniversalCameraData cameraData)
	{
		RendererList rendererList = default(RendererList);
		if (cameraData.xr.enabled)
		{
			if (cameraData.xr.singlePassEnabled)
			{
				return context.CreateSkyboxRendererList(cameraData.camera, cameraData.GetProjectionMatrix(), cameraData.GetViewMatrix(), cameraData.GetProjectionMatrix(1), cameraData.GetViewMatrix(1));
			}
			return context.CreateSkyboxRendererList(cameraData.camera, cameraData.GetProjectionMatrix(), cameraData.GetViewMatrix());
		}
		return context.CreateSkyboxRendererList(cameraData.camera);
	}

	private RendererListHandle CreateSkyBoxRendererList(RenderGraph renderGraph, UniversalCameraData cameraData)
	{
		RendererListHandle rendererListHandle = default(RendererListHandle);
		if (cameraData.xr.enabled)
		{
			if (cameraData.xr.singlePassEnabled)
			{
				return renderGraph.CreateSkyboxRendererList(in cameraData.camera, cameraData.GetProjectionMatrix(), cameraData.GetViewMatrix(), cameraData.GetProjectionMatrix(1), cameraData.GetViewMatrix(1));
			}
			return renderGraph.CreateSkyboxRendererList(in cameraData.camera, cameraData.GetProjectionMatrix(), cameraData.GetViewMatrix());
		}
		return renderGraph.CreateSkyboxRendererList(in cameraData.camera);
	}

	private static void ExecutePass(RasterCommandBuffer cmd, XRPass xr, RendererList rendererList)
	{
		if (xr.enabled && xr.singlePassEnabled)
		{
			cmd.SetSinglePassStereo(SystemInfo.supportsMultiview ? SinglePassStereoMode.Multiview : SinglePassStereoMode.Instancing);
		}
		cmd.DrawRendererList(rendererList);
		if (xr.enabled && xr.singlePassEnabled)
		{
			cmd.SetSinglePassStereo(SinglePassStereoMode.None);
		}
	}

	private void InitPassData(ref PassData passData, in XRPass xr, in RendererListHandle handle)
	{
		passData.xr = xr;
		passData.skyRendererListHandle = handle;
	}

	internal void Render(RenderGraph renderGraph, ContextContainer frameData, ScriptableRenderContext context, TextureHandle colorTarget, TextureHandle depthTarget, Material skyboxMaterial)
	{
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(universalCameraData);
		if (activeDebugHandler != null && activeDebugHandler.IsScreenClearNeeded)
		{
			return;
		}
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\DrawSkyboxPass.cs", 148);
		RendererListHandle handle = CreateSkyBoxRendererList(renderGraph, universalCameraData);
		XRPass xr = universalCameraData.xr;
		InitPassData(ref passData, in xr, in handle);
		passData.material = skyboxMaterial;
		rasterRenderGraphBuilder.UseRendererList(in handle);
		rasterRenderGraphBuilder.SetRenderAttachment(colorTarget, 0);
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(depthTarget);
		rasterRenderGraphBuilder.AllowPassCulling(value: false);
		if (universalCameraData.xr.enabled)
		{
			bool flag = universalCameraData.xrUniversal.canFoveateIntermediatePasses || universalResourceData.isActiveTargetBackBuffer;
			rasterRenderGraphBuilder.EnableFoveatedRasterization(universalCameraData.xr.supportsFoveatedRendering && flag);
		}
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			ExecutePass(context.cmd, data.xr, data.skyRendererListHandle);
		});
	}
}
