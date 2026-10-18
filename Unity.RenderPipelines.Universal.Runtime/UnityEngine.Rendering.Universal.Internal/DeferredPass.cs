using System;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

internal class DeferredPass : ScriptableRenderPass
{
	private class PassData
	{
		internal UniversalCameraData cameraData;

		internal UniversalLightData lightData;

		internal UniversalShadowData shadowData;

		internal TextureHandle color;

		internal TextureHandle depth;

		internal TextureHandle[] gbuffer;

		internal DeferredLights deferredLights;
	}

	private DeferredLights m_DeferredLights;

	public DeferredPass(RenderPassEvent evt, DeferredLights deferredLights)
	{
		base.profilingSampler = new ProfilingSampler("Render Deferred Lighting");
		base.renderPassEvent = evt;
		m_DeferredLights = deferredLights;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescripor)
	{
		RTHandle rTHandle = m_DeferredLights.GbufferAttachments[m_DeferredLights.GBufferLightingIndex];
		RTHandle rTHandle2 = m_DeferredLights.DepthAttachmentHandle;
		if (m_DeferredLights.UseFramebufferFetch)
		{
			ConfigureInputAttachments(m_DeferredLights.DeferredInputAttachments, m_DeferredLights.DeferredInputIsTransient);
		}
		ConfigureTarget(rTHandle, rTHandle2);
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		ContextContainer frameData = renderingData.frameData;
		UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		UniversalShadowData shadowData = frameData.Get<UniversalShadowData>();
		m_DeferredLights.ExecuteDeferredPass(CommandBufferHelpers.GetRasterCommandBuffer(renderingData.commandBuffer), cameraData, lightData, shadowData);
	}

	internal void Render(RenderGraph renderGraph, ContextContainer frameData, TextureHandle color, TextureHandle depth, TextureHandle[] gbuffer)
	{
		frameData.Get<UniversalResourceData>();
		UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		UniversalShadowData shadowData = frameData.Get<UniversalShadowData>();
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\DeferredPass.cs", 82);
		passData.cameraData = cameraData;
		passData.lightData = lightData;
		passData.shadowData = shadowData;
		passData.color = color;
		rasterRenderGraphBuilder.SetRenderAttachment(color, 0);
		passData.depth = depth;
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(depth);
		passData.deferredLights = m_DeferredLights;
		if (!m_DeferredLights.UseFramebufferFetch)
		{
			for (int i = 0; i < gbuffer.Length; i++)
			{
				if (i != m_DeferredLights.GBufferLightingIndex)
				{
					rasterRenderGraphBuilder.UseTexture(in gbuffer[i]);
				}
			}
		}
		else
		{
			int num = 0;
			for (int j = 0; j < gbuffer.Length; j++)
			{
				if (j != m_DeferredLights.GBufferLightingIndex)
				{
					rasterRenderGraphBuilder.SetInputAttachment(gbuffer[j], num);
					num++;
				}
			}
		}
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			data.deferredLights.ExecuteDeferredPass(context.cmd, data.cameraData, data.lightData, data.shadowData);
		});
	}

	public override void OnCameraCleanup(CommandBuffer cmd)
	{
		m_DeferredLights.OnCameraCleanup(cmd);
	}
}
