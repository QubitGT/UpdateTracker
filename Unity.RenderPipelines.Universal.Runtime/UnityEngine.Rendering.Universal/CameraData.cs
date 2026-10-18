using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

public struct CameraData
{
	private ContextContainer frameData;

	internal UniversalCameraData universalCameraData => frameData.Get<UniversalCameraData>();

	public ref Camera camera => ref frameData.Get<UniversalCameraData>().camera;

	public ref UniversalCameraHistory historyManager => ref frameData.Get<UniversalCameraData>().m_HistoryManager;

	public ref CameraRenderType renderType => ref frameData.Get<UniversalCameraData>().renderType;

	public ref RenderTexture targetTexture => ref frameData.Get<UniversalCameraData>().targetTexture;

	public ref RenderTextureDescriptor cameraTargetDescriptor => ref frameData.Get<UniversalCameraData>().cameraTargetDescriptor;

	internal ref Rect pixelRect => ref frameData.Get<UniversalCameraData>().pixelRect;

	internal ref bool useScreenCoordOverride => ref frameData.Get<UniversalCameraData>().useScreenCoordOverride;

	internal ref Vector4 screenSizeOverride => ref frameData.Get<UniversalCameraData>().screenSizeOverride;

	internal ref Vector4 screenCoordScaleBias => ref frameData.Get<UniversalCameraData>().screenCoordScaleBias;

	internal ref int pixelWidth => ref frameData.Get<UniversalCameraData>().pixelWidth;

	internal ref int pixelHeight => ref frameData.Get<UniversalCameraData>().pixelHeight;

	internal ref float aspectRatio => ref frameData.Get<UniversalCameraData>().aspectRatio;

	public ref float renderScale => ref frameData.Get<UniversalCameraData>().renderScale;

	internal ref ImageScalingMode imageScalingMode => ref frameData.Get<UniversalCameraData>().imageScalingMode;

	internal ref ImageUpscalingFilter upscalingFilter => ref frameData.Get<UniversalCameraData>().upscalingFilter;

	internal ref bool fsrOverrideSharpness => ref frameData.Get<UniversalCameraData>().fsrOverrideSharpness;

	internal ref float fsrSharpness => ref frameData.Get<UniversalCameraData>().fsrSharpness;

	internal ref HDRColorBufferPrecision hdrColorBufferPrecision => ref frameData.Get<UniversalCameraData>().hdrColorBufferPrecision;

	public ref bool clearDepth => ref frameData.Get<UniversalCameraData>().clearDepth;

	public ref CameraType cameraType => ref frameData.Get<UniversalCameraData>().cameraType;

	public ref bool isDefaultViewport => ref frameData.Get<UniversalCameraData>().isDefaultViewport;

	public ref bool isHdrEnabled => ref frameData.Get<UniversalCameraData>().isHdrEnabled;

	public ref bool allowHDROutput => ref frameData.Get<UniversalCameraData>().allowHDROutput;

	public ref bool isAlphaOutputEnabled => ref frameData.Get<UniversalCameraData>().isAlphaOutputEnabled;

	public ref bool requiresDepthTexture => ref frameData.Get<UniversalCameraData>().requiresDepthTexture;

	public ref bool requiresOpaqueTexture => ref frameData.Get<UniversalCameraData>().requiresOpaqueTexture;

	public ref bool postProcessingRequiresDepthTexture => ref frameData.Get<UniversalCameraData>().postProcessingRequiresDepthTexture;

	public ref bool xrRendering => ref frameData.Get<UniversalCameraData>().xrRendering;

	internal bool requireSrgbConversion => frameData.Get<UniversalCameraData>().requireSrgbConversion;

	public bool isSceneViewCamera => frameData.Get<UniversalCameraData>().isSceneViewCamera;

	public bool isPreviewCamera => frameData.Get<UniversalCameraData>().isPreviewCamera;

	internal bool isRenderPassSupportedCamera => frameData.Get<UniversalCameraData>().isRenderPassSupportedCamera;

	internal bool resolveToScreen => frameData.Get<UniversalCameraData>().resolveToScreen;

	public bool isHDROutputActive => frameData.Get<UniversalCameraData>().isHDROutputActive;

	public HDROutputUtils.HDRDisplayInformation hdrDisplayInformation => frameData.Get<UniversalCameraData>().hdrDisplayInformation;

	public ColorGamut hdrDisplayColorGamut => frameData.Get<UniversalCameraData>().hdrDisplayColorGamut;

	public bool rendersOverlayUI => frameData.Get<UniversalCameraData>().rendersOverlayUI;

	public ref SortingCriteria defaultOpaqueSortFlags => ref frameData.Get<UniversalCameraData>().defaultOpaqueSortFlags;

	public XRPass xr
	{
		get
		{
			return frameData.Get<UniversalCameraData>().xr;
		}
		internal set
		{
			frameData.Get<UniversalCameraData>().xr = value;
		}
	}

	internal XRPassUniversal xrUniversal => frameData.Get<UniversalCameraData>().xrUniversal;

	public ref float maxShadowDistance => ref frameData.Get<UniversalCameraData>().maxShadowDistance;

	public ref bool postProcessEnabled => ref frameData.Get<UniversalCameraData>().postProcessEnabled;

	public ref IEnumerator<Action<RenderTargetIdentifier, CommandBuffer>> captureActions => ref frameData.Get<UniversalCameraData>().captureActions;

	public ref LayerMask volumeLayerMask => ref frameData.Get<UniversalCameraData>().volumeLayerMask;

	public ref Transform volumeTrigger => ref frameData.Get<UniversalCameraData>().volumeTrigger;

	public ref bool isStopNaNEnabled => ref frameData.Get<UniversalCameraData>().isStopNaNEnabled;

	public ref bool isDitheringEnabled => ref frameData.Get<UniversalCameraData>().isDitheringEnabled;

	public ref AntialiasingMode antialiasing => ref frameData.Get<UniversalCameraData>().antialiasing;

	public ref AntialiasingQuality antialiasingQuality => ref frameData.Get<UniversalCameraData>().antialiasingQuality;

	public ref ScriptableRenderer renderer => ref frameData.Get<UniversalCameraData>().renderer;

	public ref bool resolveFinalTarget => ref frameData.Get<UniversalCameraData>().resolveFinalTarget;

	public ref Vector3 worldSpaceCameraPos => ref frameData.Get<UniversalCameraData>().worldSpaceCameraPos;

	public ref Color backgroundColor => ref frameData.Get<UniversalCameraData>().backgroundColor;

	internal ref TaaHistory taaHistory => ref frameData.Get<UniversalCameraData>().taaHistory;

	internal ref TemporalAA.Settings taaSettings => ref frameData.Get<UniversalCameraData>().taaSettings;

	internal bool resetHistory => frameData.Get<UniversalCameraData>().resetHistory;

	public ref Camera baseCamera => ref frameData.Get<UniversalCameraData>().baseCamera;

	internal CameraData(ContextContainer frameData)
	{
		this.frameData = frameData;
	}

	internal void SetViewAndProjectionMatrix(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
	{
		frameData.Get<UniversalCameraData>().SetViewAndProjectionMatrix(viewMatrix, projectionMatrix);
	}

	internal void SetViewProjectionAndJitterMatrix(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 jitterMatrix)
	{
		frameData.Get<UniversalCameraData>().SetViewProjectionAndJitterMatrix(viewMatrix, projectionMatrix, jitterMatrix);
	}

	internal void PushBuiltinShaderConstantsXR(RasterCommandBuffer cmd, bool renderIntoTexture)
	{
		frameData.Get<UniversalCameraData>().PushBuiltinShaderConstantsXR(cmd, renderIntoTexture);
	}

	public Matrix4x4 GetViewMatrix(int viewIndex = 0)
	{
		return frameData.Get<UniversalCameraData>().GetViewMatrix(viewIndex);
	}

	public Matrix4x4 GetProjectionMatrix(int viewIndex = 0)
	{
		return frameData.Get<UniversalCameraData>().GetProjectionMatrix(viewIndex);
	}

	internal Matrix4x4 GetProjectionMatrixNoJitter(int viewIndex = 0)
	{
		return frameData.Get<UniversalCameraData>().GetProjectionMatrixNoJitter(viewIndex);
	}

	public Matrix4x4 GetGPUProjectionMatrix(int viewIndex = 0)
	{
		return frameData.Get<UniversalCameraData>().GetGPUProjectionMatrix(viewIndex);
	}

	public Matrix4x4 GetGPUProjectionMatrixNoJitter(int viewIndex = 0)
	{
		return frameData.Get<UniversalCameraData>().GetGPUProjectionMatrixNoJitter(viewIndex);
	}

	internal Matrix4x4 GetGPUProjectionMatrix(bool renderIntoTexture, int viewIndex = 0)
	{
		return frameData.Get<UniversalCameraData>().GetGPUProjectionMatrix(renderIntoTexture, viewIndex);
	}

	public bool IsHandleYFlipped(RTHandle handle)
	{
		return frameData.Get<UniversalCameraData>().IsHandleYFlipped(handle);
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public bool IsCameraProjectionMatrixFlipped()
	{
		return frameData.Get<UniversalCameraData>().IsCameraProjectionMatrixFlipped();
	}

	public bool IsRenderTargetProjectionMatrixFlipped(RTHandle color, RTHandle depth = null)
	{
		return frameData.Get<UniversalCameraData>().IsRenderTargetProjectionMatrixFlipped(color, depth);
	}

	internal bool IsTemporalAAEnabled()
	{
		return frameData.Get<UniversalCameraData>().IsTemporalAAEnabled();
	}
}
