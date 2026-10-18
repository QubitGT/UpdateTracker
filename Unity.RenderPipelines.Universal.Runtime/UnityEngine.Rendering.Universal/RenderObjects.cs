using System;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.Universal;

[ExcludeFromPreset]
[MovedFrom(true, "UnityEngine.Experimental.Rendering.Universal", null, null)]
[Tooltip("Render Objects simplifies the injection of additional render passes by exposing a selection of commonly used settings.")]
public class RenderObjects : ScriptableRendererFeature
{
	[Serializable]
	public class RenderObjectsSettings
	{
		public enum OverrideMaterialMode
		{
			None,
			Material,
			Shader
		}

		public string passTag = "RenderObjectsFeature";

		public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

		public FilterSettings filterSettings = new FilterSettings();

		public Material overrideMaterial;

		public int overrideMaterialPassIndex;

		public Shader overrideShader;

		public int overrideShaderPassIndex;

		public OverrideMaterialMode overrideMode = OverrideMaterialMode.Material;

		public bool overrideDepthState;

		public CompareFunction depthCompareFunction = CompareFunction.LessEqual;

		public bool enableWrite = true;

		public StencilStateData stencilSettings = new StencilStateData();

		public CustomCameraSettings cameraSettings = new CustomCameraSettings();
	}

	[Serializable]
	public class FilterSettings
	{
		public RenderQueueType RenderQueueType;

		public LayerMask LayerMask;

		public string[] PassNames;

		public FilterSettings()
		{
			RenderQueueType = RenderQueueType.Opaque;
			LayerMask = 0;
		}
	}

	[Serializable]
	public class CustomCameraSettings
	{
		public bool overrideCamera;

		public bool restoreCamera = true;

		public Vector4 offset;

		public float cameraFieldOfView = 60f;
	}

	public RenderObjectsSettings settings = new RenderObjectsSettings();

	private RenderObjectsPass renderObjectsPass;

	public override void Create()
	{
		FilterSettings filterSettings = settings.filterSettings;
		if (settings.Event < RenderPassEvent.BeforeRenderingPrePasses)
		{
			settings.Event = RenderPassEvent.BeforeRenderingPrePasses;
		}
		renderObjectsPass = new RenderObjectsPass(settings.passTag, settings.Event, filterSettings.PassNames, filterSettings.RenderQueueType, filterSettings.LayerMask, settings.cameraSettings);
		switch (settings.overrideMode)
		{
		case RenderObjectsSettings.OverrideMaterialMode.None:
			renderObjectsPass.overrideMaterial = null;
			renderObjectsPass.overrideShader = null;
			break;
		case RenderObjectsSettings.OverrideMaterialMode.Material:
			renderObjectsPass.overrideMaterial = settings.overrideMaterial;
			renderObjectsPass.overrideMaterialPassIndex = settings.overrideMaterialPassIndex;
			renderObjectsPass.overrideShader = null;
			break;
		case RenderObjectsSettings.OverrideMaterialMode.Shader:
			renderObjectsPass.overrideMaterial = null;
			renderObjectsPass.overrideShader = settings.overrideShader;
			renderObjectsPass.overrideShaderPassIndex = settings.overrideShaderPassIndex;
			break;
		}
		if (settings.overrideDepthState)
		{
			renderObjectsPass.SetDepthState(settings.enableWrite, settings.depthCompareFunction);
		}
		if (settings.stencilSettings.overrideStencilState)
		{
			renderObjectsPass.SetStencilState(settings.stencilSettings.stencilReference, settings.stencilSettings.stencilCompareFunction, settings.stencilSettings.passOperation, settings.stencilSettings.failOperation, settings.stencilSettings.zFailOperation);
		}
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (renderingData.cameraData.cameraType != CameraType.Preview && !UniversalRenderer.IsOffscreenDepthTexture(ref renderingData.cameraData))
		{
			renderer.EnqueuePass(renderObjectsPass);
		}
	}

	internal override bool SupportsNativeRenderPass()
	{
		return true;
	}
}
