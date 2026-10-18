using System;
using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.Universal;

[MovedFrom(true, "UnityEngine.Experimental.Rendering.Universal", null, null)]
public class RenderObjectsPass : ScriptableRenderPass
{
	private class PassData
	{
		internal RenderObjects.CustomCameraSettings cameraSettings;

		internal RenderPassEvent renderPassEvent;

		internal TextureHandle color;

		internal RendererListHandle rendererListHdl;

		internal DebugRendererLists debugRendererLists;

		internal UniversalCameraData cameraData;

		internal RendererList rendererList;
	}

	private RenderQueueType renderQueueType;

	private FilteringSettings m_FilteringSettings;

	private RenderObjects.CustomCameraSettings m_CameraSettings;

	private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

	private PassData m_PassData;

	private RenderStateBlock m_RenderStateBlock;

	public Material overrideMaterial { get; set; }

	public int overrideMaterialPassIndex { get; set; }

	public Shader overrideShader { get; set; }

	public int overrideShaderPassIndex { get; set; }

	[Obsolete("Use SetDepthState instead", true)]
	public void SetDetphState(bool writeEnabled, CompareFunction function = CompareFunction.Less)
	{
		SetDepthState(writeEnabled, function);
	}

	public void SetDepthState(bool writeEnabled, CompareFunction function = CompareFunction.Less)
	{
		m_RenderStateBlock.mask |= RenderStateMask.Depth;
		m_RenderStateBlock.depthState = new DepthState(writeEnabled, function);
	}

	public void SetStencilState(int reference, CompareFunction compareFunction, StencilOp passOp, StencilOp failOp, StencilOp zFailOp)
	{
		StencilState defaultValue = StencilState.defaultValue;
		defaultValue.enabled = true;
		defaultValue.SetCompareFunction(compareFunction);
		defaultValue.SetPassOperation(passOp);
		defaultValue.SetFailOperation(failOp);
		defaultValue.SetZFailOperation(zFailOp);
		m_RenderStateBlock.mask |= RenderStateMask.Stencil;
		m_RenderStateBlock.stencilReference = reference;
		m_RenderStateBlock.stencilState = defaultValue;
	}

	public RenderObjectsPass(string profilerTag, RenderPassEvent renderPassEvent, string[] shaderTags, RenderQueueType renderQueueType, int layerMask, RenderObjects.CustomCameraSettings cameraSettings)
	{
		base.profilingSampler = new ProfilingSampler(profilerTag);
		Init(renderPassEvent, shaderTags, renderQueueType, layerMask, cameraSettings);
	}

	internal RenderObjectsPass(URPProfileId profileId, RenderPassEvent renderPassEvent, string[] shaderTags, RenderQueueType renderQueueType, int layerMask, RenderObjects.CustomCameraSettings cameraSettings)
	{
		base.profilingSampler = ProfilingSampler.Get(profileId);
		Init(renderPassEvent, shaderTags, renderQueueType, layerMask, cameraSettings);
	}

	internal void Init(RenderPassEvent renderPassEvent, string[] shaderTags, RenderQueueType renderQueueType, int layerMask, RenderObjects.CustomCameraSettings cameraSettings)
	{
		m_PassData = new PassData();
		base.renderPassEvent = renderPassEvent;
		this.renderQueueType = renderQueueType;
		overrideMaterial = null;
		overrideMaterialPassIndex = 0;
		overrideShader = null;
		overrideShaderPassIndex = 0;
		RenderQueueRange value = ((renderQueueType == RenderQueueType.Transparent) ? RenderQueueRange.transparent : RenderQueueRange.opaque);
		m_FilteringSettings = new FilteringSettings(value, layerMask);
		if (shaderTags != null && shaderTags.Length != 0)
		{
			foreach (string name in shaderTags)
			{
				m_ShaderTagIdList.Add(new ShaderTagId(name));
			}
		}
		else
		{
			m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
			m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
			m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
		}
		m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
		m_CameraSettings = cameraSettings;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		UniversalRenderingData renderingData2 = renderingData.frameData.Get<UniversalRenderingData>();
		UniversalCameraData cameraData = renderingData.frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = renderingData.frameData.Get<UniversalLightData>();
		RasterCommandBuffer rasterCommandBuffer = CommandBufferHelpers.GetRasterCommandBuffer(renderingData.commandBuffer);
		using (new ProfilingScope(rasterCommandBuffer, base.profilingSampler))
		{
			InitPassData(cameraData, ref m_PassData);
			InitRendererLists(renderingData2, lightData, ref m_PassData, context, null, useRenderGraph: false);
			ExecutePass(m_PassData, rasterCommandBuffer, m_PassData.rendererList, renderingData.cameraData.IsCameraProjectionMatrixFlipped());
		}
	}

	private static void ExecutePass(PassData passData, RasterCommandBuffer cmd, RendererList rendererList, bool isYFlipped)
	{
		Camera camera = passData.cameraData.camera;
		Rect pixelRect = passData.cameraData.pixelRect;
		float aspect = pixelRect.width / pixelRect.height;
		if (passData.cameraSettings.overrideCamera)
		{
			if (passData.cameraData.xr.enabled)
			{
				Debug.LogWarning("RenderObjects pass is configured to override camera matrices. While rendering in stereo camera matrices cannot be overridden.");
			}
			else
			{
				Matrix4x4 proj = Matrix4x4.Perspective(passData.cameraSettings.cameraFieldOfView, aspect, camera.nearClipPlane, camera.farClipPlane);
				proj = GL.GetGPUProjectionMatrix(proj, isYFlipped);
				Matrix4x4 viewMatrix = passData.cameraData.GetViewMatrix();
				Vector4 column = viewMatrix.GetColumn(3);
				viewMatrix.SetColumn(3, column + passData.cameraSettings.offset);
				RenderingUtils.SetViewAndProjectionMatrices(cmd, viewMatrix, proj, setInverseMatrices: false);
			}
		}
		if (ScriptableRenderPass.GetActiveDebugHandler(passData.cameraData) != null)
		{
			passData.debugRendererLists.DrawWithRendererList(cmd);
		}
		else
		{
			cmd.DrawRendererList(rendererList);
		}
		if (passData.cameraSettings.overrideCamera && passData.cameraSettings.restoreCamera && !passData.cameraData.xr.enabled)
		{
			RenderingUtils.SetViewAndProjectionMatrices(cmd, passData.cameraData.GetViewMatrix(), GL.GetGPUProjectionMatrix(passData.cameraData.GetProjectionMatrix(), isYFlipped), setInverseMatrices: false);
		}
	}

	private void InitPassData(UniversalCameraData cameraData, ref PassData passData)
	{
		passData.cameraSettings = m_CameraSettings;
		passData.renderPassEvent = base.renderPassEvent;
		passData.cameraData = cameraData;
	}

	private void InitRendererLists(UniversalRenderingData renderingData, UniversalLightData lightData, ref PassData passData, ScriptableRenderContext context, RenderGraph renderGraph, bool useRenderGraph)
	{
		SortingCriteria sortingCriteria = ((renderQueueType == RenderQueueType.Transparent) ? SortingCriteria.CommonTransparent : passData.cameraData.defaultOpaqueSortFlags);
		DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList, renderingData, passData.cameraData, lightData, sortingCriteria);
		drawingSettings.overrideMaterial = overrideMaterial;
		drawingSettings.overrideMaterialPassIndex = overrideMaterialPassIndex;
		drawingSettings.overrideShader = overrideShader;
		drawingSettings.overrideShaderPassIndex = overrideShaderPassIndex;
		DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(passData.cameraData);
		_ = m_FilteringSettings;
		if (useRenderGraph)
		{
			if (activeDebugHandler != null)
			{
				passData.debugRendererLists = activeDebugHandler.CreateRendererListsWithDebugRenderState(renderGraph, ref renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings, ref m_RenderStateBlock);
			}
			else
			{
				RenderingUtils.CreateRendererListWithRenderStateBlock(renderGraph, ref renderingData.cullResults, drawingSettings, m_FilteringSettings, m_RenderStateBlock, ref passData.rendererListHdl);
			}
		}
		else if (activeDebugHandler != null)
		{
			passData.debugRendererLists = activeDebugHandler.CreateRendererListsWithDebugRenderState(context, ref renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings, ref m_RenderStateBlock);
		}
		else
		{
			RenderingUtils.CreateRendererListWithRenderStateBlock(context, ref renderingData.cullResults, drawingSettings, m_FilteringSettings, m_RenderStateBlock, ref passData.rendererList);
		}
	}

	public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
	{
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\RenderObjectsPass.cs", 274);
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		InitPassData(universalCameraData, ref passData);
		passData.color = universalResourceData.activeColorTexture;
		rasterRenderGraphBuilder.SetRenderAttachment(universalResourceData.activeColorTexture, 0);
		if (universalCameraData.imageScalingMode != ImageScalingMode.Upscaling || passData.renderPassEvent != RenderPassEvent.AfterRenderingPostProcessing)
		{
			rasterRenderGraphBuilder.SetRenderAttachmentDepth(universalResourceData.activeDepthTexture);
		}
		TextureHandle mainShadowsTexture = universalResourceData.mainShadowsTexture;
		TextureHandle additionalShadowsTexture = universalResourceData.additionalShadowsTexture;
		if (mainShadowsTexture.IsValid())
		{
			rasterRenderGraphBuilder.UseTexture(in mainShadowsTexture);
		}
		if (additionalShadowsTexture.IsValid())
		{
			rasterRenderGraphBuilder.UseTexture(in additionalShadowsTexture);
		}
		TextureHandle[] dBuffer = universalResourceData.dBuffer;
		for (int i = 0; i < dBuffer.Length; i++)
		{
			TextureHandle textureHandle = dBuffer[i];
			if (textureHandle.IsValid())
			{
				rasterRenderGraphBuilder.UseTexture(in textureHandle);
			}
		}
		TextureHandle ssaoTexture = universalResourceData.ssaoTexture;
		if (ssaoTexture.IsValid())
		{
			rasterRenderGraphBuilder.UseTexture(in ssaoTexture);
		}
		InitRendererLists(renderingData, lightData, ref passData, default(ScriptableRenderContext), renderGraph, useRenderGraph: true);
		if (ScriptableRenderPass.GetActiveDebugHandler(passData.cameraData) != null)
		{
			passData.debugRendererLists.PrepareRendererListForRasterPass(rasterRenderGraphBuilder);
		}
		else
		{
			rasterRenderGraphBuilder.UseRendererList(in passData.rendererListHdl);
		}
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		if (universalCameraData.xr.enabled)
		{
			rasterRenderGraphBuilder.EnableFoveatedRasterization(universalCameraData.xr.supportsFoveatedRendering && universalCameraData.xrUniversal.canFoveateIntermediatePasses);
		}
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext rgContext)
		{
			bool isYFlipped = data.cameraData.IsRenderTargetProjectionMatrixFlipped(data.color);
			ExecutePass(data, rgContext.cmd, data.rendererListHdl, isYFlipped);
		});
	}
}
