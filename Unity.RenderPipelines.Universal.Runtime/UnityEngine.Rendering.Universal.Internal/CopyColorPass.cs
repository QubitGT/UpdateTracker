using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

public class CopyColorPass : ScriptableRenderPass
{
	private class PassData
	{
		internal TextureHandle source;

		internal TextureHandle destination;

		internal bool useProceduralBlit;

		internal Material samplingMaterial;

		internal Material copyColorMaterial;

		internal Downsampling downsamplingMethod;

		internal int sampleOffsetShaderHandle;
	}

	private int m_SampleOffsetShaderHandle;

	private Material m_SamplingMaterial;

	private Downsampling m_DownsamplingMethod;

	private Material m_CopyColorMaterial;

	private PassData m_PassData;

	private RTHandle source { get; set; }

	private RTHandle destination { get; set; }

	public CopyColorPass(RenderPassEvent evt, Material samplingMaterial, Material copyColorMaterial = null, string customPassName = null)
	{
		base.profilingSampler = ((customPassName != null) ? new ProfilingSampler(customPassName) : ProfilingSampler.Get(URPProfileId.CopyColor));
		m_PassData = new PassData();
		m_SamplingMaterial = samplingMaterial;
		m_CopyColorMaterial = copyColorMaterial;
		m_SampleOffsetShaderHandle = Shader.PropertyToID("_SampleOffset");
		base.renderPassEvent = evt;
		m_DownsamplingMethod = Downsampling.None;
		base.useNativeRenderPass = false;
	}

	public static void ConfigureDescriptor(Downsampling downsamplingMethod, ref RenderTextureDescriptor descriptor, out FilterMode filterMode)
	{
		descriptor.msaaSamples = 1;
		descriptor.depthStencilFormat = GraphicsFormat.None;
		switch (downsamplingMethod)
		{
		case Downsampling._2xBilinear:
			descriptor.width = Mathf.Max(1, descriptor.width / 2);
			descriptor.height = Mathf.Max(1, descriptor.height / 2);
			break;
		case Downsampling._4xBox:
		case Downsampling._4xBilinear:
			descriptor.width = Mathf.Max(1, descriptor.width / 4);
			descriptor.height = Mathf.Max(1, descriptor.height / 4);
			break;
		}
		filterMode = ((downsamplingMethod != 0) ? FilterMode.Bilinear : FilterMode.Point);
	}

	[Obsolete("Use RTHandles for source and destination.", true)]
	public void Setup(RenderTargetIdentifier source, RenderTargetHandle destination, Downsampling downsampling)
	{
		throw new NotSupportedException("Setup with RenderTargetIdentifier has been deprecated. Use it with RTHandles instead.");
	}

	public void Setup(RTHandle source, RTHandle destination, Downsampling downsampling)
	{
		this.source = source;
		this.destination = destination;
		m_DownsamplingMethod = downsampling;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		cmd.SetGlobalTexture(destination.name, destination.nameID);
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		m_PassData.samplingMaterial = m_SamplingMaterial;
		m_PassData.copyColorMaterial = m_CopyColorMaterial;
		m_PassData.downsamplingMethod = m_DownsamplingMethod;
		m_PassData.sampleOffsetShaderHandle = m_SampleOffsetShaderHandle;
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		if (source == renderingData.cameraData.renderer.GetCameraColorFrontBuffer(commandBuffer))
		{
			source = renderingData.cameraData.renderer.cameraColorTargetHandle;
		}
		if (renderingData.cameraData.xr.supportsFoveatedRendering)
		{
			commandBuffer.SetFoveatedRenderingMode(FoveatedRenderingMode.Disabled);
		}
		ScriptableRenderer.SetRenderTarget(commandBuffer, destination, ScriptableRenderPass.k_CameraTarget, base.clearFlag, base.clearColor);
		ExecutePass(CommandBufferHelpers.GetRasterCommandBuffer(commandBuffer), m_PassData, source, renderingData.cameraData.xr.enabled);
	}

	private static void ExecutePass(RasterCommandBuffer cmd, PassData passData, RTHandle source, bool useDrawProceduralBlit)
	{
		Material samplingMaterial = passData.samplingMaterial;
		Material copyColorMaterial = passData.copyColorMaterial;
		Downsampling downsamplingMethod = passData.downsamplingMethod;
		int sampleOffsetShaderHandle = passData.sampleOffsetShaderHandle;
		if (samplingMaterial == null)
		{
			Debug.LogErrorFormat("Missing {0}. Copy Color render pass will not execute. Check for missing reference in the renderer resources.", samplingMaterial);
			return;
		}
		using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.CopyColor)))
		{
			Vector2 vector = (source.useScaling ? new Vector2(source.rtHandleProperties.rtHandleScale.x, source.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			switch (downsamplingMethod)
			{
			case Downsampling.None:
				Blitter.BlitTexture(cmd, source, vector, copyColorMaterial, 0);
				break;
			case Downsampling._2xBilinear:
				Blitter.BlitTexture(cmd, source, vector, copyColorMaterial, 1);
				break;
			case Downsampling._4xBox:
				samplingMaterial.SetFloat(sampleOffsetShaderHandle, 2f);
				Blitter.BlitTexture(cmd, source, vector, samplingMaterial, 0);
				break;
			case Downsampling._4xBilinear:
				Blitter.BlitTexture(cmd, source, vector, copyColorMaterial, 1);
				break;
			}
		}
	}

	internal TextureHandle Render(RenderGraph renderGraph, ContextContainer frameData, out TextureHandle destination, in TextureHandle source, Downsampling downsampling)
	{
		m_DownsamplingMethod = downsampling;
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		RenderTextureDescriptor descriptor = universalCameraData.cameraTargetDescriptor;
		ConfigureDescriptor(downsampling, ref descriptor, out var filterMode);
		destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, "_CameraOpaqueTexture", clear: true, filterMode);
		RenderInternal(renderGraph, in destination, in source, universalCameraData.xr.enabled);
		return destination;
	}

	internal void RenderToExistingTexture(RenderGraph renderGraph, ContextContainer frameData, in TextureHandle destination, in TextureHandle source, Downsampling downsampling = Downsampling.None)
	{
		m_DownsamplingMethod = downsampling;
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		RenderInternal(renderGraph, in destination, in source, universalCameraData.xr.enabled);
	}

	private void RenderInternal(RenderGraph renderGraph, in TextureHandle destination, in TextureHandle source, bool useProceduralBlit)
	{
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\CopyColorPass.cs", 216);
		passData.destination = destination;
		rasterRenderGraphBuilder.SetRenderAttachment(destination, 0, AccessFlags.WriteAll);
		passData.source = source;
		rasterRenderGraphBuilder.UseTexture(in source);
		passData.useProceduralBlit = useProceduralBlit;
		passData.samplingMaterial = m_SamplingMaterial;
		passData.copyColorMaterial = m_CopyColorMaterial;
		passData.downsamplingMethod = m_DownsamplingMethod;
		passData.sampleOffsetShaderHandle = m_SampleOffsetShaderHandle;
		if (destination.IsValid())
		{
			rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in destination, Shader.PropertyToID("_CameraOpaqueTexture"));
		}
		rasterRenderGraphBuilder.AllowPassCulling(value: false);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			ExecutePass(context.cmd, data, data.source, data.useProceduralBlit);
		});
	}
}
