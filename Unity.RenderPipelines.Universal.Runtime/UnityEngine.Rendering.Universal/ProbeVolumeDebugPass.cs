using System;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class ProbeVolumeDebugPass : ScriptableRenderPass
{
	private class WriteApvData
	{
		public ComputeShader computeShader;

		public BufferHandle resultBuffer;

		public Vector2 clickCoordinates;

		public TextureHandle depthBuffer;

		public TextureHandle normalBuffer;
	}

	private ComputeShader m_ComputeShader;

	private RTHandle m_DepthTexture;

	private RTHandle m_NormalTexture;

	public ProbeVolumeDebugPass(RenderPassEvent evt, ComputeShader computeShader)
	{
		base.profilingSampler = new ProfilingSampler("Dispatch APV Debug");
		base.renderPassEvent = evt;
		m_ComputeShader = computeShader;
	}

	public void Setup(RTHandle depthBuffer, RTHandle normalBuffer)
	{
		m_DepthTexture = depthBuffer;
		m_NormalTexture = normalBuffer;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (ProbeReferenceVolume.instance.isInitialized)
		{
			ref CameraData cameraData = ref renderingData.cameraData;
			if (ProbeReferenceVolume.instance.GetProbeSamplingDebugResources(cameraData.camera, out var resultBuffer, out var coords))
			{
				CommandBuffer commandBuffer = renderingData.commandBuffer;
				int kernelIndex = m_ComputeShader.FindKernel("ComputePositionNormal");
				commandBuffer.SetComputeTextureParam(m_ComputeShader, kernelIndex, "_CameraDepthTexture", m_DepthTexture);
				commandBuffer.SetComputeTextureParam(m_ComputeShader, kernelIndex, "_NormalBufferTexture", m_NormalTexture);
				commandBuffer.SetComputeVectorParam(m_ComputeShader, "_positionSS", new Vector4(coords.x, coords.y, 0f, 0f));
				commandBuffer.SetComputeBufferParam(m_ComputeShader, kernelIndex, "_ResultBuffer", resultBuffer);
				commandBuffer.DispatchCompute(m_ComputeShader, kernelIndex, 1, 1, 1);
			}
		}
	}

	internal void Render(RenderGraph renderGraph, ContextContainer frameData, TextureHandle depthPyramidBuffer, TextureHandle normalBuffer)
	{
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		if (!ProbeReferenceVolume.instance.isInitialized || !ProbeReferenceVolume.instance.GetProbeSamplingDebugResources(universalCameraData.camera, out var resultBuffer, out var coords))
		{
			return;
		}
		WriteApvData passData;
		using IComputeRenderGraphBuilder computeRenderGraphBuilder = renderGraph.AddComputePass<WriteApvData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\ProbeVolumeDebugPass.cs", 81);
		passData.clickCoordinates = coords;
		passData.computeShader = m_ComputeShader;
		passData.resultBuffer = renderGraph.ImportBuffer(resultBuffer);
		passData.depthBuffer = depthPyramidBuffer;
		passData.normalBuffer = normalBuffer;
		computeRenderGraphBuilder.UseBuffer(in passData.resultBuffer, AccessFlags.Write);
		computeRenderGraphBuilder.UseTexture(in passData.depthBuffer);
		computeRenderGraphBuilder.UseTexture(in passData.normalBuffer);
		computeRenderGraphBuilder.SetRenderFunc(delegate(WriteApvData data, ComputeGraphContext ctx)
		{
			int kernelIndex = data.computeShader.FindKernel("ComputePositionNormal");
			ctx.cmd.SetComputeTextureParam(data.computeShader, kernelIndex, "_CameraDepthTexture", data.depthBuffer);
			ctx.cmd.SetComputeTextureParam(data.computeShader, kernelIndex, "_NormalBufferTexture", data.normalBuffer);
			ctx.cmd.SetComputeVectorParam(data.computeShader, "_positionSS", new Vector4(data.clickCoordinates.x, data.clickCoordinates.y, 0f, 0f));
			ctx.cmd.SetComputeBufferParam(data.computeShader, kernelIndex, "_ResultBuffer", data.resultBuffer);
			ctx.cmd.DispatchCompute(data.computeShader, kernelIndex, 1, 1, 1);
		});
	}
}
