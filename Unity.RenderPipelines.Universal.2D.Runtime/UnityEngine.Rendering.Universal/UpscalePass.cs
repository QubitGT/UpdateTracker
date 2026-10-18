using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class UpscalePass : ScriptableRenderPass
{
	private class PassData
	{
		internal TextureHandle source;
	}

	private static readonly string k_UpscalePass = "Upscale2D Pass";

	private static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler(k_UpscalePass);

	private static readonly ProfilingSampler m_ExecuteProfilingSampler = new ProfilingSampler("Draw Upscale");

	private static Material m_BlitMaterial;

	private RTHandle source;

	private RTHandle destination;

	public UpscalePass(RenderPassEvent evt, Material blitMaterial)
	{
		base.renderPassEvent = evt;
		m_BlitMaterial = blitMaterial;
	}

	public void Setup(RTHandle colorTargetHandle, int width, int height, FilterMode mode, RenderTextureDescriptor cameraTargetDescriptor, out RTHandle upscaleHandle)
	{
		source = colorTargetHandle;
		RenderTextureDescriptor descriptor = cameraTargetDescriptor;
		descriptor.width = width;
		descriptor.height = height;
		descriptor.depthStencilFormat = GraphicsFormat.None;
		RenderingUtils.ReAllocateHandleIfNeeded(ref destination, in descriptor, mode, TextureWrapMode.Clamp, 1, 0f, "_UpscaleTexture");
		upscaleHandle = destination;
	}

	public void Dispose()
	{
		destination?.Release();
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		commandBuffer.SetRenderTarget(destination);
		ExecutePass(CommandBufferHelpers.GetRasterCommandBuffer(commandBuffer), source);
	}

	private static void ExecutePass(RasterCommandBuffer cmd, RTHandle source)
	{
		using (new ProfilingScope(cmd, m_ExecuteProfilingSampler))
		{
			Vector2 vector = (source.useScaling ? new Vector2(source.rtHandleProperties.rtHandleScale.x, source.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Blitter.BlitTexture(cmd, source, vector, m_BlitMaterial, (source.rt.filterMode == FilterMode.Bilinear) ? 1 : 0);
		}
	}

	public void Render(RenderGraph graph, Camera camera, in TextureHandle cameraColorAttachment, in TextureHandle upscaleHandle)
	{
		camera.TryGetComponent<PixelPerfectCamera>(out var component);
		if (component == null || !component.enabled || !component.requiresUpscalePass)
		{
			return;
		}
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = graph.AddRasterRenderPass<PassData>(k_UpscalePass, out passData, m_ProfilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\2D\\Rendergraph\\UpscalePass.cs", 71);
		passData.source = cameraColorAttachment;
		rasterRenderGraphBuilder.SetRenderAttachment(upscaleHandle, 0);
		rasterRenderGraphBuilder.UseTexture(in cameraColorAttachment);
		rasterRenderGraphBuilder.AllowPassCulling(value: false);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			ExecutePass(context.cmd, data.source);
		});
	}
}
