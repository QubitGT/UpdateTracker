using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

public class UniversalCameraData : ContextItem
{
	private Matrix4x4 m_ViewMatrix;

	private Matrix4x4 m_ProjectionMatrix;

	private Matrix4x4 m_JitterMatrix;

	private bool m_CachedRenderIntoTextureXR;

	private bool m_InitBuiltinXRConstants;

	public Camera camera;

	internal UniversalCameraHistory m_HistoryManager;

	public CameraRenderType renderType;

	public RenderTexture targetTexture;

	public RenderTextureDescriptor cameraTargetDescriptor;

	internal Rect pixelRect;

	internal bool useScreenCoordOverride;

	internal Vector4 screenSizeOverride;

	internal Vector4 screenCoordScaleBias;

	internal int pixelWidth;

	internal int pixelHeight;

	internal float aspectRatio;

	public float renderScale;

	internal ImageScalingMode imageScalingMode;

	internal ImageUpscalingFilter upscalingFilter;

	internal bool fsrOverrideSharpness;

	internal float fsrSharpness;

	internal HDRColorBufferPrecision hdrColorBufferPrecision;

	public bool clearDepth;

	public CameraType cameraType;

	public bool isDefaultViewport;

	public bool isHdrEnabled;

	public bool allowHDROutput;

	public bool isAlphaOutputEnabled;

	public bool requiresDepthTexture;

	public bool requiresOpaqueTexture;

	public bool postProcessingRequiresDepthTexture;

	public bool xrRendering;

	internal bool useGPUOcclusionCulling;

	internal bool stackLastCameraOutputToHDR;

	public SortingCriteria defaultOpaqueSortFlags;

	public float maxShadowDistance;

	public bool postProcessEnabled;

	internal bool stackAnyPostProcessingEnabled;

	public IEnumerator<Action<RenderTargetIdentifier, CommandBuffer>> captureActions;

	public LayerMask volumeLayerMask;

	public Transform volumeTrigger;

	public bool isStopNaNEnabled;

	public bool isDitheringEnabled;

	public AntialiasingMode antialiasing;

	public AntialiasingQuality antialiasingQuality;

	public ScriptableRenderer renderer;

	public bool resolveFinalTarget;

	public Vector3 worldSpaceCameraPos;

	public Color backgroundColor;

	internal TaaHistory taaHistory;

	internal StpHistory stpHistory;

	internal TemporalAA.Settings taaSettings;

	public Camera baseCamera;

	internal bool isLastBaseCamera;

	public int scaledWidth => Mathf.Max(1, (int)((float)camera.pixelWidth * renderScale));

	public int scaledHeight => Mathf.Max(1, (int)((float)camera.pixelHeight * renderScale));

	public UniversalCameraHistory historyManager
	{
		get
		{
			return m_HistoryManager;
		}
		set
		{
			m_HistoryManager = value;
		}
	}

	internal bool requireSrgbConversion
	{
		get
		{
			if (xr.enabled)
			{
				if (!xr.renderTargetDesc.sRGB && (xr.renderTargetDesc.graphicsFormat == GraphicsFormat.R8G8B8A8_UNorm || xr.renderTargetDesc.graphicsFormat == GraphicsFormat.B8G8R8A8_UNorm))
				{
					return QualitySettings.activeColorSpace == ColorSpace.Linear;
				}
				return false;
			}
			if (targetTexture == null)
			{
				return Display.main.requiresSrgbBlitToBackbuffer;
			}
			return false;
		}
	}

	public bool isGameCamera => cameraType == CameraType.Game;

	public bool isSceneViewCamera => cameraType == CameraType.SceneView;

	public bool isPreviewCamera => cameraType == CameraType.Preview;

	internal bool isRenderPassSupportedCamera
	{
		get
		{
			if (cameraType != CameraType.Game)
			{
				return cameraType == CameraType.Reflection;
			}
			return true;
		}
	}

	internal bool resolveToScreen
	{
		get
		{
			if (targetTexture == null && resolveFinalTarget)
			{
				if (cameraType != CameraType.Game)
				{
					return camera.cameraType == CameraType.VR;
				}
				return true;
			}
			return false;
		}
	}

	public bool isHDROutputActive
	{
		get
		{
			bool flag = UniversalRenderPipeline.HDROutputForMainDisplayIsActive();
			if (xr.enabled)
			{
				flag = xr.isHDRDisplayOutputActive;
			}
			if (flag && allowHDROutput)
			{
				return resolveToScreen;
			}
			return false;
		}
	}

	public HDROutputUtils.HDRDisplayInformation hdrDisplayInformation
	{
		get
		{
			if (xr.enabled)
			{
				return xr.hdrDisplayOutputInformation;
			}
			HDROutputSettings main = HDROutputSettings.main;
			return new HDROutputUtils.HDRDisplayInformation(main.maxFullFrameToneMapLuminance, main.maxToneMapLuminance, main.minToneMapLuminance, main.paperWhiteNits);
		}
	}

	public ColorGamut hdrDisplayColorGamut
	{
		get
		{
			if (xr.enabled)
			{
				return xr.hdrDisplayOutputColorGamut;
			}
			return HDROutputSettings.main.displayColorGamut;
		}
	}

	public bool rendersOverlayUI
	{
		get
		{
			if (SupportedRenderingFeatures.active.rendersUIOverlay)
			{
				return resolveToScreen;
			}
			return false;
		}
	}

	public XRPass xr { get; internal set; }

	internal XRPassUniversal xrUniversal => xr as XRPassUniversal;

	internal bool resetHistory => taaSettings.resetHistoryFrames != 0;

	internal void SetViewAndProjectionMatrix(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
	{
		m_ViewMatrix = viewMatrix;
		m_ProjectionMatrix = projectionMatrix;
		m_JitterMatrix = Matrix4x4.identity;
	}

	internal void SetViewProjectionAndJitterMatrix(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 jitterMatrix)
	{
		m_ViewMatrix = viewMatrix;
		m_ProjectionMatrix = projectionMatrix;
		m_JitterMatrix = jitterMatrix;
	}

	internal void PushBuiltinShaderConstantsXR(RasterCommandBuffer cmd, bool renderIntoTexture)
	{
		if ((!m_InitBuiltinXRConstants || m_CachedRenderIntoTextureXR != renderIntoTexture || !xr.singlePassEnabled) && xr.enabled)
		{
			Matrix4x4 projectionMatrix = GetProjectionMatrix();
			Matrix4x4 viewMatrix = GetViewMatrix();
			cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
			if (xr.singlePassEnabled)
			{
				Matrix4x4 projectionMatrix2 = GetProjectionMatrix(1);
				Matrix4x4 viewMatrix2 = GetViewMatrix(1);
				XRBuiltinShaderConstants.UpdateBuiltinShaderConstants(viewMatrix, projectionMatrix, renderIntoTexture, 0);
				XRBuiltinShaderConstants.UpdateBuiltinShaderConstants(viewMatrix2, projectionMatrix2, renderIntoTexture, 1);
				XRBuiltinShaderConstants.SetBuiltinShaderConstants(cmd);
			}
			else
			{
				Vector3 vector = Matrix4x4.Inverse(GetViewMatrix()).GetColumn(3);
				cmd.SetGlobalVector(ShaderPropertyId.worldSpaceCameraPos, vector);
				Matrix4x4 gPUProjectionMatrix = GetGPUProjectionMatrix(renderIntoTexture);
				Matrix4x4 matrix4x = Matrix4x4.Inverse(viewMatrix);
				Matrix4x4 matrix4x2 = Matrix4x4.Inverse(gPUProjectionMatrix);
				Matrix4x4 value = matrix4x * matrix4x2;
				Matrix4x4 value2 = Matrix4x4.Scale(new Vector3(1f, 1f, -1f)) * viewMatrix;
				Matrix4x4 inverse = value2.inverse;
				cmd.SetGlobalMatrix(ShaderPropertyId.worldToCameraMatrix, value2);
				cmd.SetGlobalMatrix(ShaderPropertyId.cameraToWorldMatrix, inverse);
				cmd.SetGlobalMatrix(ShaderPropertyId.inverseViewMatrix, matrix4x);
				cmd.SetGlobalMatrix(ShaderPropertyId.inverseProjectionMatrix, matrix4x2);
				cmd.SetGlobalMatrix(ShaderPropertyId.inverseViewAndProjectionMatrix, value);
			}
			m_CachedRenderIntoTextureXR = renderIntoTexture;
			m_InitBuiltinXRConstants = true;
		}
	}

	public Matrix4x4 GetViewMatrix(int viewIndex = 0)
	{
		if (xr.enabled)
		{
			return xr.GetViewMatrix(viewIndex);
		}
		return m_ViewMatrix;
	}

	public Matrix4x4 GetProjectionMatrix(int viewIndex = 0)
	{
		if (xr.enabled)
		{
			return m_JitterMatrix * xr.GetProjMatrix(viewIndex);
		}
		return m_JitterMatrix * m_ProjectionMatrix;
	}

	internal Matrix4x4 GetProjectionMatrixNoJitter(int viewIndex = 0)
	{
		if (xr.enabled)
		{
			return xr.GetProjMatrix(viewIndex);
		}
		return m_ProjectionMatrix;
	}

	public Matrix4x4 GetGPUProjectionMatrix(int viewIndex = 0)
	{
		return m_JitterMatrix * GL.GetGPUProjectionMatrix(GetProjectionMatrixNoJitter(viewIndex), IsCameraProjectionMatrixFlipped());
	}

	public Matrix4x4 GetGPUProjectionMatrixNoJitter(int viewIndex = 0)
	{
		return GL.GetGPUProjectionMatrix(GetProjectionMatrixNoJitter(viewIndex), IsCameraProjectionMatrixFlipped());
	}

	internal Matrix4x4 GetGPUProjectionMatrix(bool renderIntoTexture, int viewIndex = 0)
	{
		return GL.GetGPUProjectionMatrix(GetProjectionMatrix(viewIndex), renderIntoTexture);
	}

	public bool IsHandleYFlipped(RTHandle handle)
	{
		if (!SystemInfo.graphicsUVStartsAtTop)
		{
			return true;
		}
		if (cameraType == CameraType.SceneView || cameraType == CameraType.Preview)
		{
			return true;
		}
		RenderTargetIdentifier renderTargetIdentifier = new RenderTargetIdentifier(handle.nameID, 0);
		bool flag = renderTargetIdentifier == BuiltinRenderTextureType.CameraTarget || renderTargetIdentifier == BuiltinRenderTextureType.Depth;
		if (xr.enabled)
		{
			flag |= renderTargetIdentifier == new RenderTargetIdentifier(xr.renderTarget, 0);
		}
		return !flag;
	}

	public bool IsCameraProjectionMatrixFlipped()
	{
		if (!SystemInfo.graphicsUVStartsAtTop)
		{
			return false;
		}
		ScriptableRenderer current = ScriptableRenderer.current;
		if (current != null)
		{
			if (!IsHandleYFlipped(current.cameraColorTargetHandle))
			{
				return targetTexture != null;
			}
			return true;
		}
		return true;
	}

	public bool IsRenderTargetProjectionMatrixFlipped(RTHandle color, RTHandle depth = null)
	{
		if (!SystemInfo.graphicsUVStartsAtTop)
		{
			return true;
		}
		if (!(targetTexture != null))
		{
			return IsHandleYFlipped(color ?? depth);
		}
		return true;
	}

	internal bool IsTemporalAARequested()
	{
		return antialiasing == AntialiasingMode.TemporalAntiAliasing;
	}

	internal bool IsTemporalAAEnabled()
	{
		camera.TryGetComponent<UniversalAdditionalCameraData>(out var component);
		if (IsTemporalAARequested() && postProcessEnabled && taaHistory != null && cameraTargetDescriptor.msaaSamples == 1 && ((object)component == null || component.renderType != CameraRenderType.Overlay) && ((object)component == null || component.cameraStack.Count <= 0) && !camera.allowDynamicResolution)
		{
			return renderer.SupportsMotionVectors();
		}
		return false;
	}

	internal bool IsSTPRequested()
	{
		if (imageScalingMode == ImageScalingMode.Upscaling)
		{
			return upscalingFilter == ImageUpscalingFilter.STP;
		}
		return false;
	}

	internal bool IsSTPEnabled()
	{
		if (IsSTPRequested())
		{
			return IsTemporalAAEnabled();
		}
		return false;
	}

	public override void Reset()
	{
		m_ViewMatrix = default(Matrix4x4);
		m_ProjectionMatrix = default(Matrix4x4);
		m_JitterMatrix = default(Matrix4x4);
		m_CachedRenderIntoTextureXR = false;
		m_InitBuiltinXRConstants = false;
		camera = null;
		renderType = CameraRenderType.Base;
		targetTexture = null;
		cameraTargetDescriptor = default(RenderTextureDescriptor);
		pixelRect = default(Rect);
		useScreenCoordOverride = false;
		screenSizeOverride = default(Vector4);
		screenCoordScaleBias = default(Vector4);
		pixelWidth = 0;
		pixelHeight = 0;
		aspectRatio = 0f;
		renderScale = 1f;
		imageScalingMode = ImageScalingMode.None;
		upscalingFilter = ImageUpscalingFilter.Point;
		fsrOverrideSharpness = false;
		fsrSharpness = 0f;
		hdrColorBufferPrecision = HDRColorBufferPrecision._32Bits;
		clearDepth = false;
		cameraType = CameraType.Game;
		isDefaultViewport = false;
		isHdrEnabled = false;
		allowHDROutput = false;
		isAlphaOutputEnabled = false;
		requiresDepthTexture = false;
		requiresOpaqueTexture = false;
		postProcessingRequiresDepthTexture = false;
		xrRendering = false;
		useGPUOcclusionCulling = false;
		defaultOpaqueSortFlags = SortingCriteria.None;
		xr = null;
		maxShadowDistance = 0f;
		postProcessEnabled = false;
		captureActions = null;
		volumeLayerMask = 0;
		volumeTrigger = null;
		isStopNaNEnabled = false;
		isDitheringEnabled = false;
		antialiasing = AntialiasingMode.None;
		antialiasingQuality = AntialiasingQuality.Low;
		renderer = null;
		resolveFinalTarget = false;
		worldSpaceCameraPos = default(Vector3);
		backgroundColor = Color.black;
		taaHistory = null;
		stpHistory = null;
		taaSettings = default(TemporalAA.Settings);
		baseCamera = null;
		isLastBaseCamera = false;
		stackAnyPostProcessingEnabled = false;
		stackLastCameraOutputToHDR = false;
	}
}
