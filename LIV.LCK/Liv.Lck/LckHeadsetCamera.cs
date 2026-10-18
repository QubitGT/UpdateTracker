using System;
using System.Collections.Generic;
using Liv.Lck.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace Liv.Lck;

public class LckHeadsetCamera : MonoBehaviour, ILckCamera
{
	[SerializeField]
	private string _cameraId;

	[SerializeField]
	private Camera _xrCamera;

	[SerializeField]
	private EyeSelection _eye;

	[SerializeField]
	private HeadsetCropMode _cropMode;

	[SerializeField]
	private Material _blitMaterial;

	internal static readonly List<LckHeadsetCamera> _activeInstances = new List<LckHeadsetCamera>();

	private RenderTexture _intermediateRT;

	private bool _useTextureArray;

	private bool _useSRP;

	private bool _flipY;

	private bool _isMultiPass;

	private bool _stereoModeDetected;

	private bool _captureInitialized;

	private Camera _resolvedCamera;

	private int _lastRenderFeatureCaptureFrame = -1;

	private CommandBuffer _cmd;

	private Material _materialInstance;

	private int _cachedSrcW;

	private int _cachedSrcH;

	private int _cachedDstW;

	private int _cachedDstH;

	private HeadsetCropMode _cachedCropMode;

	private const string CmdBufferName = "LCK Headset Capture";

	private const int LegacyBlitPassIndex = 0;

	private static readonly int SliceIndexId = Shader.PropertyToID("_SliceIndex");

	private static readonly int ScaleOffsetId = Shader.PropertyToID("_ScaleOffset");

	private static readonly int FlipYId = Shader.PropertyToID("_FlipY");

	public string CameraId => _cameraId;

	public EyeSelection Eye
	{
		get
		{
			return _eye;
		}
		set
		{
			_eye = value;
			if (_useTextureArray && _materialInstance != null)
			{
				_materialInstance.SetFloat(SliceIndexId, (float)_eye);
			}
		}
	}

	public HeadsetCropMode CropMode
	{
		get
		{
			return _cropMode;
		}
		set
		{
			_cropMode = value;
			InvalidateScaleOffsetCache();
		}
	}

	internal bool IsActive { get; private set; }

	internal RenderTexture ActiveTargetTexture { get; private set; }

	internal Material MaterialInstance => _materialInstance;

	internal bool UseTextureArrayBlit
	{
		get
		{
			if (_stereoModeDetected && _useTextureArray)
			{
				return !_isMultiPass;
			}
			return false;
		}
	}

	private void Awake()
	{
		if (string.IsNullOrEmpty(_cameraId))
		{
			_cameraId = Guid.NewGuid().ToString();
		}
		_useTextureArray = SystemInfo.supports2DArrayTextures && _blitMaterial != null;
		_useSRP = GraphicsSettings.defaultRenderPipeline != null;
		_flipY = SystemInfo.graphicsUVStartsAtTop;
		_cmd = new CommandBuffer
		{
			name = "LCK Headset Capture"
		};
		if (_useTextureArray)
		{
			_materialInstance = new Material(_blitMaterial);
			_materialInstance.SetFloat(SliceIndexId, (float)_eye);
			_materialInstance.SetFloat(FlipYId, _flipY ? 1f : 0f);
		}
		else if (_blitMaterial == null)
		{
			LckLog.LogWarning("LckHeadsetCamera: No blit material assigned — falling back to simple blit. Eye selection will not work.", "Awake", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckHeadsetCamera.cs", 121);
		}
		else if (!SystemInfo.supports2DArrayTextures)
		{
			LckLog.LogWarning("LckHeadsetCamera: Device does not support 2D array textures — falling back to simple blit. Eye selection will not work.", "Awake", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckHeadsetCamera.cs", 123);
		}
		LckMediator.RegisterCamera(this);
		if (_useSRP)
		{
			RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
		}
		else
		{
			Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(OnPostRenderBuiltIn));
		}
	}

	private void OnDestroy()
	{
		if (_useSRP)
		{
			RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
		}
		else
		{
			Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(OnPostRenderBuiltIn));
		}
		if (IsActive)
		{
			IsActive = false;
			ActiveTargetTexture = null;
			_activeInstances.Remove(this);
		}
		_cmd?.Release();
		_cmd = null;
		if (_materialInstance != null)
		{
			UnityEngine.Object.Destroy(_materialInstance);
			_materialInstance = null;
		}
		LckMediator.UnregisterCamera(this);
		ReleaseIntermediateRT();
	}

	public void ActivateCamera(RenderTexture renderTexture)
	{
		ActiveTargetTexture = renderTexture;
		IsActive = true;
		if (!_activeInstances.Contains(this))
		{
			_activeInstances.Add(this);
		}
		if (!_captureInitialized)
		{
			InitializeCapture();
		}
	}

	private void InitializeCapture()
	{
		_captureInitialized = true;
		DetectStereoModeIfNeeded();
		if (!_stereoModeDetected)
		{
			LckLog.LogWarning("LckHeadsetCamera: XR device is not active — stereo rendering mode could not be detected. Headset capture may not work correctly.", "InitializeCapture", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckHeadsetCamera.cs", 188);
		}
		if (_useSRP && !LckHeadsetCaptureAutoSetup.EnsureFeaturePresent())
		{
			LckLog.LogWarning("LckHeadsetCamera: Failed to auto-add LckHeadsetCaptureRenderFeature to URP renderer. Headset capture will use the endCameraRendering fallback.", "InitializeCapture", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckHeadsetCamera.cs", 195);
		}
	}

	public void DeactivateCamera()
	{
		IsActive = false;
		ActiveTargetTexture = null;
		_activeInstances.Remove(this);
	}

	public Camera GetCameraComponent()
	{
		if (_resolvedCamera == null)
		{
			_resolvedCamera = ((_xrCamera != null) ? _xrCamera : Camera.main);
		}
		return _resolvedCamera;
	}

	internal bool IsTargetCamera(Camera cam)
	{
		if (_xrCamera != null)
		{
			return cam == _xrCamera;
		}
		if (cam.stereoEnabled)
		{
			return true;
		}
		return cam == Camera.main;
	}

	internal void MarkCapturedByRenderFeature()
	{
		_lastRenderFeatureCaptureFrame = Time.frameCount;
	}

	private void OnEndCameraRendering(ScriptableRenderContext context, Camera cam)
	{
		if (IsActive && !(ActiveTargetTexture == null) && _lastRenderFeatureCaptureFrame != Time.frameCount && IsTargetCamera(cam) && ShouldCaptureEye(cam))
		{
			_cmd.Clear();
			PopulateCommandBuffer(_cmd, cam);
			context.ExecuteCommandBuffer(_cmd);
			context.Submit();
		}
	}

	private void OnPostRenderBuiltIn(Camera cam)
	{
		if (IsActive && !(ActiveTargetTexture == null) && IsTargetCamera(cam) && ShouldCaptureEye(cam))
		{
			_cmd.Clear();
			PopulateCommandBuffer(_cmd, cam);
			Graphics.ExecuteCommandBuffer(_cmd);
		}
	}

	private void DetectStereoModeIfNeeded()
	{
		if (!_stereoModeDetected && XRSettings.isDeviceActive)
		{
			_isMultiPass = XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.MultiPass;
			_stereoModeDetected = true;
		}
	}

	internal bool ShouldCaptureEye(Camera cam)
	{
		if (!_isMultiPass)
		{
			return true;
		}
		Camera.MonoOrStereoscopicEye stereoActiveEye = cam.stereoActiveEye;
		if (stereoActiveEye == Camera.MonoOrStereoscopicEye.Mono)
		{
			return true;
		}
		Camera.MonoOrStereoscopicEye monoOrStereoscopicEye = ((_eye != 0) ? Camera.MonoOrStereoscopicEye.Right : Camera.MonoOrStereoscopicEye.Left);
		return stereoActiveEye == monoOrStereoscopicEye;
	}

	internal void UpdateMaterialForCapture(Camera cam, bool isSourceBackBuffer)
	{
		if (_materialInstance != null)
		{
			bool flag = _flipY && !isSourceBackBuffer;
			_materialInstance.SetFloat(FlipYId, flag ? 1f : 0f);
		}
		if (_useTextureArray && !_isMultiPass)
		{
			int srcW = ((XRSettings.eyeTextureWidth > 0) ? XRSettings.eyeTextureWidth : cam.pixelWidth);
			int srcH = ((XRSettings.eyeTextureHeight > 0) ? XRSettings.eyeTextureHeight : cam.pixelHeight);
			UpdateScaleOffsetIfNeeded(srcW, srcH);
		}
	}

	internal void PopulateCommandBuffer(CommandBuffer cmd, Camera cam)
	{
		if (_useTextureArray && !_isMultiPass)
		{
			int srcW = ((XRSettings.eyeTextureWidth > 0) ? XRSettings.eyeTextureWidth : cam.pixelWidth);
			int srcH = ((XRSettings.eyeTextureHeight > 0) ? XRSettings.eyeTextureHeight : cam.pixelHeight);
			UpdateScaleOffsetIfNeeded(srcW, srcH);
			cmd.Blit(BuiltinRenderTextureType.CameraTarget, ActiveTargetTexture, _materialInstance, 0);
		}
		else
		{
			PrepareIntermediateRT(cam);
			cmd.Blit(BuiltinRenderTextureType.CameraTarget, _intermediateRT);
			Vector4 vector = ((_cropMode == HeadsetCropMode.ZoomFill) ? ComputeScaleOffset(_intermediateRT.width, _intermediateRT.height) : new Vector4(1f, 1f, 0f, 0f));
			float y = (_flipY ? (0f - vector.y) : vector.y);
			float y2 = (_flipY ? (vector.w + vector.y) : vector.w);
			cmd.Blit(_intermediateRT, ActiveTargetTexture, new Vector2(vector.x, y), new Vector2(vector.z, y2));
		}
	}

	private void UpdateScaleOffsetIfNeeded(int srcW, int srcH)
	{
		int width = ActiveTargetTexture.width;
		int height = ActiveTargetTexture.height;
		if (srcW != _cachedSrcW || srcH != _cachedSrcH || width != _cachedDstW || height != _cachedDstH || _cropMode != _cachedCropMode)
		{
			_cachedSrcW = srcW;
			_cachedSrcH = srcH;
			_cachedDstW = width;
			_cachedDstH = height;
			_cachedCropMode = _cropMode;
			_materialInstance.SetVector(ScaleOffsetId, ComputeScaleOffset(srcW, srcH));
		}
	}

	private void InvalidateScaleOffsetCache()
	{
		_cachedSrcW = 0;
		_cachedSrcH = 0;
	}

	private Vector4 ComputeScaleOffset(int srcW, int srcH)
	{
		float num = (float)srcW / (float)srcH;
		float num2 = (float)ActiveTargetTexture.width / (float)ActiveTargetTexture.height;
		float num4;
		float z;
		float num5;
		float w;
		if (_cropMode == HeadsetCropMode.ZoomFill)
		{
			if (num < num2)
			{
				float num3 = num / num2;
				num4 = 1f;
				z = 0f;
				num5 = num3;
				w = (1f - num3) * 0.5f;
			}
			else
			{
				float num6 = num2 / num;
				num4 = num6;
				z = (1f - num6) * 0.5f;
				num5 = 1f;
				w = 0f;
			}
		}
		else if (num < num2)
		{
			float num7 = num / num2;
			num4 = 1f / num7;
			z = (0f - (1f - num7)) * 0.5f * num4;
			num5 = 1f;
			w = 0f;
		}
		else
		{
			float num8 = num2 / num;
			num4 = 1f;
			z = 0f;
			num5 = 1f / num8;
			w = (0f - (1f - num8)) * 0.5f * num5;
		}
		return new Vector4(num4, num5, z, w);
	}

	private void PrepareIntermediateRT(Camera cam)
	{
		int pixelWidth = cam.pixelWidth;
		int pixelHeight = cam.pixelHeight;
		if (!(_intermediateRT != null) || _intermediateRT.width != pixelWidth || _intermediateRT.height != pixelHeight)
		{
			ReleaseIntermediateRT();
			RenderTextureDescriptor renderTextureDescriptor = new RenderTextureDescriptor(pixelWidth, pixelHeight, ActiveTargetTexture.graphicsFormat, 0);
			renderTextureDescriptor.msaaSamples = 1;
			RenderTextureDescriptor desc = renderTextureDescriptor;
			_intermediateRT = RenderTexture.GetTemporary(desc);
		}
	}

	private void ReleaseIntermediateRT()
	{
		if (_intermediateRT != null)
		{
			RenderTexture.ReleaseTemporary(_intermediateRT);
			_intermediateRT = null;
		}
	}
}
