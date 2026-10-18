using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace Meta.XR.EnvironmentDepth;

public class EnvironmentDepthManager : MonoBehaviour
{
	private class Mask
	{
		internal readonly Material _maskMaterial;

		private readonly RenderTexture _maskDepthRt;

		private readonly RenderTexture _maskedDepthTexture;

		private readonly CommandBuffer _maskCommandBuffer;

		private readonly Matrix4x4[] _mvpMatrices = new Matrix4x4[2];

		internal Mask(int width, int height, float bias)
		{
			Shader shader = Shader.Find("Meta/EnvironmentDepth/DepthMask");
			_maskMaterial = new Material(shader)
			{
				enableInstancing = true
			};
			_maskMaterial.SetFloat(MaskBiasID, bias);
			_maskDepthRt = new RenderTexture(width, height, GraphicsFormat.R16_UNorm, GraphicsFormat.D16_UNorm)
			{
				dimension = TextureDimension.Tex2DArray,
				volumeDepth = 2
			};
			_maskedDepthTexture = new RenderTexture(width, height, GraphicsFormat.R16_UNorm, GraphicsFormat.None)
			{
				dimension = TextureDimension.Tex2DArray,
				volumeDepth = 2,
				depth = 0
			};
			_maskCommandBuffer = new CommandBuffer();
		}

		internal RenderTexture ApplyMask(RenderTexture depthTexture, List<MeshFilter> meshFilters, Matrix4x4 trackingSpaceWorldToLocal, DepthFrameDesc[] frameDescriptors)
		{
			EnvironmentDepthUtils.CalculateDepthCameraMatrices(frameDescriptors[0], out var projMatrix, out var viewMatrix);
			EnvironmentDepthUtils.CalculateDepthCameraMatrices(frameDescriptors[1], out var projMatrix2, out var viewMatrix2);
			_maskCommandBuffer.SetRenderTarget(new RenderTargetIdentifier(_maskDepthRt, 0, CubemapFace.Unknown, -1), RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
			_maskCommandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, Color.white);
			foreach (MeshFilter meshFilter in meshFilters)
			{
				if (meshFilter == null || meshFilter.sharedMesh == null)
				{
					UnityEngine.Debug.LogError("MeshFilter or sharedMesh is null.");
					continue;
				}
				_mvpMatrices[0] = GL.GetGPUProjectionMatrix(projMatrix, renderIntoTexture: true) * viewMatrix * trackingSpaceWorldToLocal * meshFilter.transform.localToWorldMatrix;
				_mvpMatrices[1] = GL.GetGPUProjectionMatrix(projMatrix2, renderIntoTexture: true) * viewMatrix2 * trackingSpaceWorldToLocal * meshFilter.transform.localToWorldMatrix;
				_maskCommandBuffer.SetGlobalMatrixArray(MvpMatricesID, _mvpMatrices);
				_maskCommandBuffer.DrawMeshInstancedProcedural(meshFilter.sharedMesh, 0, _maskMaterial, 0, 2);
			}
			_maskMaterial.SetTexture(DepthTextureID, depthTexture);
			_maskMaterial.SetTexture(MaskTextureID, _maskDepthRt);
			_maskCommandBuffer.SetRenderTarget(new RenderTargetIdentifier(_maskedDepthTexture, 0, CubemapFace.Unknown, -1), RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
			_maskCommandBuffer.DrawProcedural(Matrix4x4.identity, _maskMaterial, 1, MeshTopology.Triangles, 3, 2);
			Graphics.ExecuteCommandBuffer(_maskCommandBuffer);
			_maskCommandBuffer.Clear();
			return _maskedDepthTexture;
		}

		internal void Dispose()
		{
			UnityEngine.Object.Destroy(_maskMaterial);
			UnityEngine.Object.Destroy(_maskDepthRt);
			UnityEngine.Object.Destroy(_maskedDepthTexture);
			_maskCommandBuffer.Dispose();
		}
	}

	public const string HardOcclusionKeyword = "HARD_OCCLUSION";

	public const string SoftOcclusionKeyword = "SOFT_OCCLUSION";

	private const int numViews = 2;

	private static readonly int DepthTextureID = Shader.PropertyToID("_EnvironmentDepthTexture");

	private static readonly int ReprojectionMatricesID = Shader.PropertyToID("_EnvironmentDepthReprojectionMatrices");

	private static readonly int ZBufferParamsID = Shader.PropertyToID("_EnvironmentDepthZBufferParams");

	private static readonly int PreprocessedEnvironmentDepthTexture = Shader.PropertyToID("_PreprocessedEnvironmentDepthTexture");

	private static readonly int MvpMatricesID = Shader.PropertyToID("_DepthMask_MVP_Matrices");

	private static readonly int MaskTextureID = Shader.PropertyToID("_MaskTexture");

	private static readonly int MaskBiasID = Shader.PropertyToID("_MaskBias");

	[SerializeField]
	private OcclusionShadersMode _occlusionShadersMode = OcclusionShadersMode.SoftOcclusion;

	[SerializeField]
	[Tooltip("If set to true, hands will be removed from the depth texture.")]
	private bool _removeHands;

	[SerializeField]
	public Transform CustomTrackingSpace;

	private bool _isCameraRigCached;

	[SerializeField]
	[HideInInspector]
	private OVRCameraRig _cameraRig;

	private static IDepthProvider _provider;

	private bool _hasPermission;

	private Material _preprocessMaterial;

	[CanBeNull]
	private RenderTexture _preprocessTexture;

	private RenderTargetSetup _preprocessRenderTargetSetup;

	internal readonly DepthFrameDesc[] frameDescriptors = new DepthFrameDesc[2];

	private float _maskBias = 0.1f;

	private Mask _mask;

	private readonly Matrix4x4[] _reprojectionMatrices = new Matrix4x4[2];

	[field: SerializeField]
	public List<MeshFilter> MaskMeshFilters { get; set; } = new List<MeshFilter>();


	private static IDepthProvider provider => _provider ?? (_provider = CreateProvider());

	public static bool IsSupported => provider.IsSupported;

	public bool IsDepthAvailable { get; private set; }

	public OcclusionShadersMode OcclusionShadersMode
	{
		get
		{
			return _occlusionShadersMode;
		}
		set
		{
			if (_occlusionShadersMode != value)
			{
				_occlusionShadersMode = value;
				if (IsDepthAvailable)
				{
					SetOcclusionShaderKeywords(value);
				}
			}
		}
	}

	public bool RemoveHands
	{
		get
		{
			return _removeHands;
		}
		set
		{
			if (_removeHands != value)
			{
				_removeHands = value;
				if (base.enabled && IsSupported)
				{
					provider.RemoveHands = value;
				}
			}
		}
	}

	public float MaskBias
	{
		get
		{
			return _maskBias;
		}
		set
		{
			_maskBias = value;
			if (_mask != null)
			{
				_mask._maskMaterial.SetFloat(MaskBiasID, value);
			}
		}
	}

	internal event Action<RenderTexture> onDepthTextureUpdate;

	[NotNull]
	private static IDepthProvider CreateProvider()
	{
		if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null && XRGeneralSettings.Instance.Manager.activeLoader != null)
		{
			XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRDisplaySubsystem>();
		}
		UnityEngine.Debug.LogError("EnvironmentDepth is disabled. Please enable XR provider in 'Project Settings / XR Plug-in Management'.");
		return new DepthProviderNotSupported();
	}

	private void Awake()
	{
		if (IsSupported)
		{
			Shader shader = Shader.Find("Meta/EnvironmentDepth/Preprocessing");
			_preprocessMaterial = new Material(shader);
		}
	}

	private void OnEnable()
	{
		Application.onBeforeRender += OnBeforeRender;
		if (!IsSupported)
		{
			UnityEngine.Debug.LogError("Environment Depth is not supported. Please check EnvironmentDepthManager.IsSupported before enabling EnvironmentDepthManager.\nOpen 'Oculus -> Tools -> Project Setup Tool' to see requirements.\n");
			base.enabled = false;
			return;
		}
		_hasPermission = Permission.HasUserAuthorizedPermission("com.oculus.permission.USE_SCENE");
		if (_hasPermission)
		{
			provider.SetDepthEnabled(isEnabled: true, _removeHands);
		}
	}

	private void ResetDepthTextureIfAvailable()
	{
		if (IsDepthAvailable)
		{
			IsDepthAvailable = false;
			Shader.SetGlobalTexture(DepthTextureID, null);
			if (_occlusionShadersMode != 0)
			{
				SetOcclusionShaderKeywords(OcclusionShadersMode.None);
			}
		}
	}

	private void OnDisable()
	{
		Application.onBeforeRender -= OnBeforeRender;
		ResetDepthTextureIfAvailable();
		if (IsSupported && _hasPermission)
		{
			provider.SetDepthEnabled(isEnabled: false, removeHands: false);
		}
	}

	private void OnDestroy()
	{
		if (_preprocessMaterial != null)
		{
			UnityEngine.Object.Destroy(_preprocessMaterial);
		}
		if (_preprocessTexture != null)
		{
			UnityEngine.Object.Destroy(_preprocessTexture);
		}
		_mask?.Dispose();
	}

	private void OnBeforeRender()
	{
		if (!_hasPermission)
		{
			if (!Permission.HasUserAuthorizedPermission("com.oculus.permission.USE_SCENE"))
			{
				return;
			}
			_hasPermission = true;
			provider.SetDepthEnabled(isEnabled: true, _removeHands);
		}
		Matrix4x4 trackingSpaceWorldToLocalMatrix = GetTrackingSpaceWorldToLocalMatrix();
		TryFetchDepthTexture(trackingSpaceWorldToLocalMatrix);
		if (IsDepthAvailable)
		{
			DepthFrameDesc depthFrameDesc = frameDescriptors[0];
			Vector4 value = EnvironmentDepthUtils.ComputeNdcToLinearDepthParameters(depthFrameDesc.nearZ, depthFrameDesc.farZ);
			Shader.SetGlobalVector(ZBufferParamsID, value);
			for (int i = 0; i < 2; i++)
			{
				_reprojectionMatrices[i] = EnvironmentDepthUtils.CalculateReprojection(frameDescriptors[i]) * trackingSpaceWorldToLocalMatrix;
			}
			Shader.SetGlobalMatrixArray(ReprojectionMatricesID, _reprojectionMatrices);
		}
	}

	private void CacheCameraRig()
	{
		if (_cameraRig == null)
		{
			_cameraRig = UnityEngine.Object.FindObjectOfType<OVRCameraRig>();
		}
	}

	private static void SetOcclusionShaderKeywords(OcclusionShadersMode mode)
	{
		switch (mode)
		{
		case OcclusionShadersMode.HardOcclusion:
			Shader.DisableKeyword("SOFT_OCCLUSION");
			Shader.EnableKeyword("HARD_OCCLUSION");
			break;
		case OcclusionShadersMode.SoftOcclusion:
			Shader.DisableKeyword("HARD_OCCLUSION");
			Shader.EnableKeyword("SOFT_OCCLUSION");
			break;
		case OcclusionShadersMode.None:
			Shader.DisableKeyword("HARD_OCCLUSION");
			Shader.DisableKeyword("SOFT_OCCLUSION");
			break;
		default:
			UnityEngine.Debug.LogError(string.Format("Environment Depth: unknown {0} {1}", "OcclusionShadersMode", mode));
			break;
		}
	}

	private void TryFetchDepthTexture(Matrix4x4 trackingSpaceWorldToLocal)
	{
		if (!provider.TryGetUpdatedDepthTexture(out var depthTexture, frameDescriptors))
		{
			return;
		}
		if (depthTexture == null)
		{
			ResetDepthTextureIfAvailable();
			return;
		}
		this.onDepthTextureUpdate?.Invoke(depthTexture);
		if (MaskMeshFilters != null && MaskMeshFilters.Count > 0)
		{
			if (_mask == null)
			{
				_mask = new Mask(depthTexture.width, depthTexture.height, _maskBias);
			}
			depthTexture = _mask.ApplyMask(depthTexture, MaskMeshFilters, trackingSpaceWorldToLocal, frameDescriptors);
		}
		Shader.SetGlobalTexture(DepthTextureID, depthTexture);
		if (!IsDepthAvailable)
		{
			IsDepthAvailable = true;
			if (_occlusionShadersMode != 0)
			{
				SetOcclusionShaderKeywords(_occlusionShadersMode);
			}
		}
		if (_occlusionShadersMode == OcclusionShadersMode.SoftOcclusion)
		{
			PreprocessDepthTexture(depthTexture);
		}
	}

	internal Matrix4x4 GetTrackingSpaceWorldToLocalMatrix()
	{
		if (CustomTrackingSpace != null)
		{
			return CustomTrackingSpace.worldToLocalMatrix;
		}
		if (!_isCameraRigCached)
		{
			_isCameraRigCached = true;
			CacheCameraRig();
		}
		if (!(_cameraRig != null))
		{
			return Matrix4x4.identity;
		}
		return _cameraRig.trackingSpace.worldToLocalMatrix;
	}

	private void PreprocessDepthTexture(RenderTexture depthTexture)
	{
		if (_preprocessTexture == null)
		{
			_preprocessTexture = new RenderTexture(depthTexture.width, depthTexture.height, GraphicsFormat.R16G16B16A16_SFloat, GraphicsFormat.None)
			{
				dimension = TextureDimension.Tex2DArray,
				volumeDepth = 2,
				name = "_preprocessTexture",
				depth = 0
			};
			_preprocessTexture.Create();
			Shader.SetGlobalTexture(PreprocessedEnvironmentDepthTexture, _preprocessTexture);
			_preprocessRenderTargetSetup = new RenderTargetSetup
			{
				color = new RenderBuffer[1] { _preprocessTexture.colorBuffer },
				depth = _preprocessTexture.depthBuffer,
				depthSlice = -1,
				colorLoad = new RenderBufferLoadAction[1] { RenderBufferLoadAction.DontCare },
				colorStore = new RenderBufferStoreAction[1],
				depthLoad = RenderBufferLoadAction.DontCare,
				depthStore = RenderBufferStoreAction.DontCare,
				mipLevel = 0,
				cubemapFace = CubemapFace.Unknown
			};
		}
		Graphics.SetRenderTarget(_preprocessRenderTargetSetup);
		_preprocessMaterial.SetPass(0);
		Graphics.DrawProceduralNow(MeshTopology.Triangles, 3, 2);
	}

	[Conditional("UNITY_ASSERTIONS")]
	private static void Log(LogType type, string msg)
	{
		UnityEngine.Debug.unityLogger.Log(type, msg);
	}
}
