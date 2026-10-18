using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

public class XROcclusionMeshPass : ScriptableRenderPass
{
	private class PassData
	{
		internal XRPass xr;

		internal TextureHandle cameraColorAttachment;

		internal TextureHandle cameraDepthAttachment;

		internal bool isActiveTargetBackBuffer;
	}

	private PassData m_PassData;

	public bool m_IsActiveTargetBackBuffer;

	public XROcclusionMeshPass(RenderPassEvent evt)
	{
		base.profilingSampler = new ProfilingSampler("Draw XR Occlusion Mesh");
		base.renderPassEvent = evt;
		m_PassData = new PassData();
		m_IsActiveTargetBackBuffer = false;
	}

	private static void ExecutePass(RasterCommandBuffer cmd, PassData data)
	{
		if (data.xr.hasValidOcclusionMesh)
		{
			if (data.isActiveTargetBackBuffer)
			{
				cmd.SetViewport(data.xr.GetViewport());
			}
			data.xr.RenderOcclusionMesh(cmd, !data.isActiveTargetBackBuffer);
		}
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		m_PassData.xr = renderingData.cameraData.xr;
		m_PassData.isActiveTargetBackBuffer = m_IsActiveTargetBackBuffer;
		ExecutePass(CommandBufferHelpers.GetRasterCommandBuffer(renderingData.commandBuffer), m_PassData);
	}

	internal void Render(RenderGraph renderGraph, ContextContainer frameData, in TextureHandle cameraColorAttachment, in TextureHandle cameraDepthAttachment)
	{
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\XROcclusionMeshPass.cs", 61);
		passData.xr = universalCameraData.xr;
		passData.cameraColorAttachment = cameraColorAttachment;
		rasterRenderGraphBuilder.SetRenderAttachment(cameraColorAttachment, 0);
		passData.cameraDepthAttachment = cameraDepthAttachment;
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(cameraDepthAttachment);
		passData.isActiveTargetBackBuffer = universalResourceData.isActiveTargetBackBuffer;
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		if (universalCameraData.xr.enabled)
		{
			bool flag = universalCameraData.xrUniversal.canFoveateIntermediatePasses || universalResourceData.isActiveTargetBackBuffer;
			rasterRenderGraphBuilder.EnableFoveatedRasterization(universalCameraData.xr.supportsFoveatedRendering && flag);
		}
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			ExecutePass(context.cmd, data);
		});
	}
}
