using System;
using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class CapturePass : ScriptableRenderPass
{
	private class UnsafePassData
	{
		internal TextureHandle source;

		public IEnumerator<Action<RenderTargetIdentifier, CommandBuffer>> captureActions;
	}

	private RTHandle m_CameraColorHandle;

	public CapturePass(RenderPassEvent evt)
	{
		base.profilingSampler = new ProfilingSampler("Capture Camera output");
		base.renderPassEvent = evt;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		m_CameraColorHandle = renderingData.cameraData.renderer.GetCameraColorBackBuffer(commandBuffer);
		using (new ProfilingScope(commandBuffer, base.profilingSampler))
		{
			RenderTargetIdentifier nameID = m_CameraColorHandle.nameID;
			IEnumerator<Action<RenderTargetIdentifier, CommandBuffer>> captureActions = renderingData.cameraData.captureActions;
			captureActions.Reset();
			while (captureActions.MoveNext())
			{
				captureActions.Current(nameID, renderingData.commandBuffer);
			}
		}
	}

	public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
	{
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		UnsafePassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<UnsafePassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\CapturePass.cs", 55);
		passData.source = universalResourceData.cameraColor;
		passData.captureActions = universalCameraData.captureActions;
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		TextureHandle cameraColor = universalResourceData.cameraColor;
		unsafeRenderGraphBuilder.UseTexture(in cameraColor);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(UnsafePassData data, UnsafeGraphContext unsafeContext)
		{
			CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(unsafeContext.cmd);
			IEnumerator<Action<RenderTargetIdentifier, CommandBuffer>> captureActions = data.captureActions;
			data.captureActions.Reset();
			while (data.captureActions.MoveNext())
			{
				captureActions.Current(data.source, nativeCommandBuffer);
			}
		});
	}
}
