using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class DrawScreenSpaceUIPass : ScriptableRenderPass
{
	private class PassData
	{
		internal RendererListHandle rendererList;
	}

	private class UnsafePassData
	{
		internal RendererListHandle rendererList;

		internal TextureHandle colorTarget;
	}

	private PassData m_PassData;

	private RTHandle m_ColorTarget;

	private RTHandle m_DepthTarget;

	private bool m_RenderOffscreen;

	private static readonly int s_CameraDepthTextureID = Shader.PropertyToID("_CameraDepthTexture");

	private static readonly int s_CameraOpaqueTextureID = Shader.PropertyToID("_CameraOpaqueTexture");

	public DrawScreenSpaceUIPass(RenderPassEvent evt, bool renderOffscreen)
	{
		base.profilingSampler = ProfilingSampler.Get(URPProfileId.DrawScreenSpaceUI);
		base.renderPassEvent = evt;
		base.useNativeRenderPass = false;
		m_RenderOffscreen = renderOffscreen;
		m_PassData = new PassData();
	}

	public static void ConfigureColorDescriptor(ref RenderTextureDescriptor descriptor, int cameraWidth, int cameraHeight)
	{
		descriptor.graphicsFormat = GraphicsFormat.R8G8B8A8_SRGB;
		descriptor.depthStencilFormat = GraphicsFormat.None;
		descriptor.width = cameraWidth;
		descriptor.height = cameraHeight;
	}

	public static void ConfigureDepthDescriptor(ref RenderTextureDescriptor descriptor, GraphicsFormat depthStencilFormat, int cameraWidth, int cameraHeight)
	{
		descriptor.graphicsFormat = GraphicsFormat.None;
		descriptor.depthStencilFormat = depthStencilFormat;
		descriptor.width = cameraWidth;
		descriptor.height = cameraHeight;
	}

	private static void ExecutePass(RasterCommandBuffer commandBuffer, PassData passData, RendererList rendererList)
	{
		commandBuffer.DrawRendererList(rendererList);
	}

	private static void ExecutePass(UnsafeCommandBuffer commandBuffer, UnsafePassData passData, RendererList rendererList)
	{
		commandBuffer.DrawRendererList(rendererList);
	}

	public void Dispose()
	{
		m_ColorTarget?.Release();
		m_DepthTarget?.Release();
	}

	public void Setup(UniversalCameraData cameraData, GraphicsFormat depthStencilFormat)
	{
		if (m_RenderOffscreen)
		{
			RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
			ConfigureColorDescriptor(ref descriptor, cameraData.pixelWidth, cameraData.pixelHeight);
			RenderingUtils.ReAllocateHandleIfNeeded(ref m_ColorTarget, in descriptor, FilterMode.Point, TextureWrapMode.Repeat, 1, 0f, "_OverlayUITexture");
			RenderTextureDescriptor descriptor2 = cameraData.cameraTargetDescriptor;
			ConfigureDepthDescriptor(ref descriptor2, depthStencilFormat, cameraData.pixelWidth, cameraData.pixelHeight);
			RenderingUtils.ReAllocateHandleIfNeeded(ref m_DepthTarget, in descriptor2, FilterMode.Point, TextureWrapMode.Repeat, 1, 0f, "_OverlayUITexture_Depth");
		}
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		if (m_RenderOffscreen)
		{
			ConfigureTarget(m_ColorTarget, m_DepthTarget);
			ConfigureClear(ClearFlag.Color, Color.clear);
			cmd?.SetGlobalTexture(ShaderPropertyId.overlayUITexture, m_ColorTarget);
			return;
		}
		UniversalCameraData universalCameraData = renderingData.frameData.Get<UniversalCameraData>();
		DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(universalCameraData);
		if (activeDebugHandler != null && activeDebugHandler.WriteToDebugScreenTexture(universalCameraData.resolveFinalTarget))
		{
			ConfigureTarget(activeDebugHandler.DebugScreenColorHandle, activeDebugHandler.DebugScreenDepthHandle);
			return;
		}
		RTHandleStaticHelpers.SetRTHandleStaticWrapper(RenderingUtils.GetCameraTargetIdentifier(ref renderingData));
		RTHandle s_RTHandleWrapper = RTHandleStaticHelpers.s_RTHandleWrapper;
		ConfigureTarget(s_RTHandleWrapper);
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		using (new ProfilingScope(renderingData.commandBuffer, base.profilingSampler))
		{
			RendererList rendererList = context.CreateUIOverlayRendererList(renderingData.cameraData.camera);
			ExecutePass(CommandBufferHelpers.GetRasterCommandBuffer(renderingData.commandBuffer), m_PassData, rendererList);
		}
	}

	internal void RenderOffscreen(RenderGraph renderGraph, ContextContainer frameData, GraphicsFormat depthStencilFormat, out TextureHandle output)
	{
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		RenderTextureDescriptor descriptor = universalCameraData.cameraTargetDescriptor;
		ConfigureColorDescriptor(ref descriptor, universalCameraData.pixelWidth, universalCameraData.pixelHeight);
		output = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, "_OverlayUITexture", clear: true);
		RenderTextureDescriptor descriptor2 = universalCameraData.cameraTargetDescriptor;
		ConfigureDepthDescriptor(ref descriptor2, depthStencilFormat, universalCameraData.pixelWidth, universalCameraData.pixelHeight);
		TextureHandle tex = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor2, "_OverlayUITexture_Depth", clear: false);
		PassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>("Draw Screen Space UIToolkit/uGUI - Offscreen", out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\DrawScreenSpaceUIPass.cs", 181))
		{
			rasterRenderGraphBuilder.UseAllGlobalTextures(enable: true);
			rasterRenderGraphBuilder.SetRenderAttachment(output, 0);
			PassData passData2 = passData;
			ref Camera camera = ref universalCameraData.camera;
			UISubset uiSubset = UISubset.UIToolkit_UGUI;
			passData2.rendererList = renderGraph.CreateUIOverlayRendererList(in camera, in uiSubset);
			rasterRenderGraphBuilder.UseRendererList(in passData.rendererList);
			rasterRenderGraphBuilder.SetRenderAttachmentDepth(tex, AccessFlags.ReadWrite);
			if (output.IsValid())
			{
				rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in output, ShaderPropertyId.overlayUITexture);
			}
			rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
			{
				ExecutePass(context.cmd, data, data.rendererList);
			});
		}
		UnsafePassData passData3;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<UnsafePassData>("Draw Screen Space IMGUI/SoftwareCursor - Offscreen", out passData3, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\DrawScreenSpaceUIPass.cs", 205);
		passData3.colorTarget = output;
		unsafeRenderGraphBuilder.UseTexture(in output, AccessFlags.Write);
		UnsafePassData unsafePassData = passData3;
		ref Camera camera2 = ref universalCameraData.camera;
		UISubset uiSubset = UISubset.LowLevel;
		unsafePassData.rendererList = renderGraph.CreateUIOverlayRendererList(in camera2, in uiSubset);
		unsafeRenderGraphBuilder.UseRendererList(in passData3.rendererList);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(UnsafePassData data, UnsafeGraphContext context)
		{
			context.cmd.SetRenderTarget(data.colorTarget);
			ExecutePass(context.cmd, data, data.rendererList);
		});
	}

	internal void RenderOverlay(RenderGraph renderGraph, ContextContainer frameData, in TextureHandle colorBuffer, in TextureHandle depthBuffer)
	{
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		frameData.Get<UniversalResourceData>();
		_ = universalCameraData.renderer;
		PassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>("Draw UIToolkit/uGUI Overlay", out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\DrawScreenSpaceUIPass.cs", 228))
		{
			rasterRenderGraphBuilder.UseAllGlobalTextures(enable: true);
			rasterRenderGraphBuilder.SetRenderAttachment(colorBuffer, 0);
			rasterRenderGraphBuilder.SetRenderAttachmentDepth(depthBuffer, AccessFlags.ReadWrite);
			PassData passData2 = passData;
			ref Camera camera = ref universalCameraData.camera;
			UISubset uiSubset = UISubset.UIToolkit_UGUI;
			passData2.rendererList = renderGraph.CreateUIOverlayRendererList(in camera, in uiSubset);
			rasterRenderGraphBuilder.UseRendererList(in passData.rendererList);
			rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
			{
				ExecutePass(context.cmd, data, data.rendererList);
			});
		}
		UnsafePassData passData3;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<UnsafePassData>("Draw IMGUI/SoftwareCursor Overlay", out passData3, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\DrawScreenSpaceUIPass.cs", 248);
		passData3.colorTarget = colorBuffer;
		unsafeRenderGraphBuilder.UseTexture(in colorBuffer, AccessFlags.Write);
		UnsafePassData unsafePassData = passData3;
		ref Camera camera2 = ref universalCameraData.camera;
		UISubset uiSubset = UISubset.LowLevel;
		unsafePassData.rendererList = renderGraph.CreateUIOverlayRendererList(in camera2, in uiSubset);
		unsafeRenderGraphBuilder.UseRendererList(in passData3.rendererList);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(UnsafePassData data, UnsafeGraphContext context)
		{
			context.cmd.SetRenderTarget(data.colorTarget);
			ExecutePass(context.cmd, data, data.rendererList);
		});
	}
}
