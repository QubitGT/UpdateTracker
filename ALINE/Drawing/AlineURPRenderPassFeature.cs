using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Drawing;

public class AlineURPRenderPassFeature : ScriptableRendererFeature
{
	public class AlineURPRenderPass : ScriptableRenderPass
	{
		private class PassData
		{
			public Camera camera;
		}

		[Obsolete]
		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
		}

		[Obsolete]
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			DrawingManager.instance.ExecuteCustomRenderPass(context, renderingData.cameraData.camera);
		}

		public AlineURPRenderPass()
		{
			base.profilingSampler = new ProfilingSampler("ALINE");
		}

		public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
		{
			UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
			UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
			PassData passData;
			using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>("ALINE", out passData, base.profilingSampler, "C:\\Users\\root\\GT\\Assets\\ALINE\\AlineURPRenderPassFeature.cs", 41);
			bool allowDisablingWireframe = false;
			if (Application.isEditor && (universalCameraData.cameraType & (CameraType.SceneView | CameraType.Preview)) != 0)
			{
				rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
				allowDisablingWireframe = true;
			}
			rasterRenderGraphBuilder.SetRenderAttachment(universalResourceData.activeColorTexture, 0);
			rasterRenderGraphBuilder.SetRenderAttachmentDepth(universalResourceData.activeDepthTexture);
			passData.camera = universalCameraData.camera;
			rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
			{
				DrawingManager.instance.ExecuteCustomRenderGraphPass(new DrawingData.CommandBufferWrapper
				{
					cmd2 = context.cmd,
					allowDisablingWireframe = allowDisablingWireframe
				}, data.camera);
			});
		}

		public override void FrameCleanup(CommandBuffer cmd)
		{
		}
	}

	private AlineURPRenderPass m_ScriptablePass;

	public override void Create()
	{
		m_ScriptablePass = new AlineURPRenderPass();
		m_ScriptablePass.renderPassEvent = (RenderPassEvent)549;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		AddRenderPasses(renderer);
	}

	public void AddRenderPasses(ScriptableRenderer renderer)
	{
		renderer.EnqueuePass(m_ScriptablePass);
	}
}
