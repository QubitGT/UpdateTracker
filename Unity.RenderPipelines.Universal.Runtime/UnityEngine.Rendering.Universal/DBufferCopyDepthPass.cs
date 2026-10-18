using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.Universal;

internal class DBufferCopyDepthPass : CopyDepthPass
{
	public DBufferCopyDepthPass(RenderPassEvent evt, Shader copyDepthShader, bool shouldClear = false, bool copyToDepth = false, bool copyResolvedDepth = false)
		: base(evt, copyDepthShader, shouldClear, copyToDepth, copyResolvedDepth)
	{
	}

	public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
	{
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		UniversalRenderer universalRenderer = universalCameraData.renderer as UniversalRenderer;
		RenderTargetInfo renderTargetInfo = renderGraph.GetRenderTargetInfo(universalResourceData.activeDepthTexture);
		bool useDepthPriming = universalRenderer.useDepthPriming;
		bool flag = renderTargetInfo.msaaSamples > 1;
		if (!useDepthPriming || flag)
		{
			TextureHandle textureHandle = (useDepthPriming ? universalResourceData.cameraDepth : universalResourceData.cameraDepthTexture);
			RenderTextureDescriptor cameraTargetDescriptor = universalCameraData.cameraTargetDescriptor;
			cameraTargetDescriptor.graphicsFormat = GraphicsFormat.None;
			cameraTargetDescriptor.depthStencilFormat = universalCameraData.cameraTargetDescriptor.depthStencilFormat;
			cameraTargetDescriptor.msaaSamples = 1;
			universalResourceData.dBufferDepth = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor, DBufferRenderPass.s_DBufferDepthName, clear: true);
			base.CopyToDepth = true;
			Render(renderGraph, universalResourceData.dBufferDepth, textureHandle, universalResourceData, universalCameraData, bindAsCameraDepth: false, "Copy DBuffer Depth");
		}
	}
}
