using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.XR;

[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class OVROverlayCanvas : OVRRayTransformer
{
	public enum DrawMode
	{
		Opaque = 0,
		OpaqueWithClip = 1,
		Transparent = 2,
		[Obsolete("Deprecated. Use Transparent", false)]
		TransparentDefaultAlpha = 2,
		[Obsolete("Deprecated. Use Transparent", false)]
		TransparentCorrectAlpha = 3,
		AlphaToMask = 4
	}

	public enum CanvasShape
	{
		Flat,
		Curved
	}

	public struct ScopedCallback : IDisposable
	{
		public event Action OnDispose;

		void IDisposable.Dispose()
		{
			this.OnDispose?.Invoke();
		}
	}

	private const float kOptimalResolutionScale = 2f;

	private Camera _camera;

	private OVROverlay _overlay;

	private MeshRenderer _meshRenderer;

	private OVROverlayMeshGenerator _meshGenerator;

	private RenderTexture _renderTexture;

	private Material _imposterMaterial;

	private bool _optimalResolutionInitialized;

	private float _optimalResolutionWidth;

	private float _optimalResolutionHeight;

	private int _lastPixelWidth;

	private int _lastPixelHeight;

	private Vector2 _imposterTextureOffset;

	private Vector2 _imposterTextureScale;

	private bool _frameIsReady;

	private bool _useTempRT;

	[SerializeField]
	internal bool _enableMipmapping;

	[SerializeField]
	internal bool _dynamicResolution = true;

	[SerializeField]
	internal int _redrawResolutionThreshold = int.MaxValue;

	public RectTransform rectTransform;

	[FormerlySerializedAs("MaxTextureSize")]
	public int maxTextureSize = 2048;

	public bool manualRedraw;

	[FormerlySerializedAs("DrawRate")]
	public int renderInterval = 1;

	[FormerlySerializedAs("DrawFrameOffset")]
	public int renderIntervalFrameOffset;

	[FormerlySerializedAs("Expensive")]
	public bool expensive;

	[FormerlySerializedAs("Layer")]
	public int layer = 5;

	[FormerlySerializedAs("Opacity")]
	public DrawMode opacity = DrawMode.Transparent;

	public CanvasShape shape;

	public float curveRadius = 1f;

	public bool overlapMask;

	public OVROverlay.OverlayType overlayType = OVROverlay.OverlayType.Underlay;

	[SerializeField]
	internal bool _overlayEnabled = true;

	private static readonly Plane[] _FrustumPlanes = new Plane[6];

	private static readonly Vector3[] _Corners = new Vector3[4];

	private bool _nonUniformScaleWarningShown;

	private (int frameCount, float? score) _lastViewPriorityScore = (frameCount: -1, score: null);

	private int CanvasRenderLayer => OVROverlayCanvasSettings.Instance.CanvasRenderLayer;

	private bool ShouldScaleViewport => _dynamicResolution;

	public bool IsCanvasPriority => OVROverlayCanvasManager.Instance?.IsCanvasPriority(this) ?? false;

	public bool ShouldShowImposter
	{
		get
		{
			if (IsCanvasPriority && overlayEnabled)
			{
				return overlayType == OVROverlay.OverlayType.Underlay;
			}
			return true;
		}
	}

	public bool overlayEnabled
	{
		get
		{
			return _overlayEnabled;
		}
		set
		{
			if ((bool)_overlay && Application.isPlaying)
			{
				_overlay.enabled = value;
				_imposterMaterial.color = (value ? Color.black : Color.white);
			}
			_overlayEnabled = value;
		}
	}

	public OVROverlay Overlay => _overlay;

	private void Start()
	{
		if (rectTransform == null)
		{
			rectTransform = GetComponent<RectTransform>();
		}
		HideFlags hideFlags = HideFlags.HideAndDontSave;
		GameObject gameObject = new GameObject(base.name + " Overlay Camera")
		{
			hideFlags = hideFlags
		};
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		_camera = gameObject.AddComponent<Camera>();
		_camera.stereoTargetEye = StereoTargetEyeMask.None;
		_camera.transform.position = base.transform.position - _camera.transform.forward;
		_camera.orthographic = true;
		_camera.enabled = false;
		_camera.clearFlags = CameraClearFlags.Color;
		_camera.backgroundColor = Color.clear;
		_camera.nearClipPlane = 0.99f;
		_camera.farClipPlane = 1.01f;
		GameObject gameObject2 = new GameObject(base.name + " Imposter")
		{
			hideFlags = hideFlags
		};
		gameObject2.transform.SetParent(base.transform, worldPositionStays: false);
		gameObject2.AddComponent<MeshFilter>();
		_meshRenderer = gameObject2.AddComponent<MeshRenderer>();
		_meshGenerator = gameObject2.AddComponent<OVROverlayMeshGenerator>();
		GameObject gameObject3 = new GameObject(base.name + " Overlay")
		{
			hideFlags = hideFlags
		};
		gameObject3.transform.SetParent(base.transform, worldPositionStays: false);
		_overlay = gameObject3.AddComponent<OVROverlay>();
		_overlay.enabled = false;
		_overlay.isDynamic = true;
		UpdateOverlaySettings();
		_useTempRT = Application.isMobilePlatform;
		InitializeRenderTexture();
	}

	private static string ToSimpleJson<T>(T value)
	{
		ref T reference = ref value;
		T val = default(T);
		object obj;
		if (val == null)
		{
			val = reference;
			reference = ref val;
			if (val == null)
			{
				obj = null;
				goto IL_0042;
			}
		}
		obj = reference.GetType();
		goto IL_0042;
		IL_0042:
		object result;
		if (obj == null || ((Type)obj).IsValueType)
		{
			if (value is bool)
			{
				object obj2 = value;
				return ((bool)((obj2 is bool) ? obj2 : null)) ? "true" : "false";
			}
			if (value is Enum || value is string)
			{
				return $"\"{value}\"";
			}
			ref T reference2 = ref value;
			val = default(T);
			if (val == null)
			{
				val = reference2;
				reference2 = ref val;
				if (val == null)
				{
					result = null;
					goto IL_0106;
				}
			}
			result = reference2.ToString();
			goto IL_0106;
		}
		PropertyInfo[] properties = value.GetType().GetProperties();
		if (properties.Length == 0)
		{
			return "{}";
		}
		IEnumerable<string> values = properties.Select((PropertyInfo p) => "\"" + p.Name + "\":" + ToSimpleJson(p.GetValue(value)));
		return "{" + string.Join(",", values) + "}";
		IL_0106:
		return (string)result;
	}

	public void UpdateOverlaySettings()
	{
		InitializeRenderTexture();
		_meshRenderer.enabled = ShouldShowImposter;
		_overlay.noDepthBufferTesting = ShouldShowImposter;
		_overlay.isAlphaPremultiplied = true;
		_overlay.currentOverlayType = overlayType;
		_overlay.enabled = overlayEnabled;
	}

	private void InitializeRenderTexture()
	{
		if (rectTransform == null)
		{
			rectTransform = GetComponent<RectTransform>();
		}
		float width = rectTransform.rect.width;
		float height = rectTransform.rect.height;
		float num = ((width >= height) ? 1f : (width / height));
		float num2 = ((height >= width) ? 1f : (height / width));
		int num3 = ((!ShouldScaleViewport) ? 8 : 0);
		int num4 = Mathf.CeilToInt(num * (float)(maxTextureSize - num3 * 2));
		int num5 = Mathf.CeilToInt(num2 * (float)(maxTextureSize - num3 * 2));
		int num6 = num4 + num3 * 2;
		int num7 = num5 + num3 * 2;
		float x = width * ((float)num6 / (float)num4);
		float num8 = height * ((float)num7 / (float)num5);
		if (_renderTexture == null || _renderTexture.width != num6 || _renderTexture.height != num7)
		{
			if (_renderTexture != null)
			{
				UnityEngine.Object.DestroyImmediate(_renderTexture);
			}
			RenderTextureDescriptor desc = new RenderTextureDescriptor(num6, num7, GraphicsFormat.R8G8B8A8_SRGB, GraphicsFormat.D24_UNorm_S8_UInt);
			bool autoGenerateMips = (desc.useMipMap = _enableMipmapping);
			desc.autoGenerateMips = autoGenerateMips;
			_renderTexture = new RenderTexture(desc);
			_renderTexture.filterMode = FilterMode.Trilinear;
			_renderTexture.name = base.name;
		}
		_camera.orthographicSize = 0.5f * num8 * GetRectTransformScale().y;
		_camera.targetTexture = _renderTexture;
		_camera.cullingMask = 1 << CanvasRenderLayer;
		Shader shader = OVROverlayCanvasSettings.Instance.GetShader(opacity);
		if (_imposterMaterial == null)
		{
			_imposterMaterial = new Material(shader);
		}
		else
		{
			_imposterMaterial.shader = shader;
		}
		if (opacity == DrawMode.OpaqueWithClip)
		{
			_imposterMaterial.EnableKeyword("WITH_CLIP");
		}
		else
		{
			_imposterMaterial.DisableKeyword("WITH_CLIP");
		}
		if (expensive)
		{
			_imposterMaterial.EnableKeyword("EXPENSIVE");
		}
		else
		{
			_imposterMaterial.DisableKeyword("EXPENSIVE");
		}
		if (opacity == DrawMode.AlphaToMask)
		{
			_imposterMaterial.EnableKeyword("ALPHA_TO_MASK");
			_imposterMaterial.SetInt("_AlphaToMask", 1);
		}
		else
		{
			_imposterMaterial.DisableKeyword("ALPHA_TO_MASK");
			_imposterMaterial.SetInt("_AlphaToMask", 0);
		}
		if (overlayEnabled && overlapMask)
		{
			_imposterMaterial.EnableKeyword("OVERLAP_MASK");
		}
		else
		{
			_imposterMaterial.DisableKeyword("OVERLAP_MASK");
		}
		_imposterMaterial.mainTexture = _renderTexture;
		_imposterMaterial.color = CalcImposterColor();
		_imposterMaterial.mainTextureOffset = _imposterTextureOffset;
		_imposterMaterial.mainTextureScale = _imposterTextureScale;
		_meshRenderer.sharedMaterial = _imposterMaterial;
		_meshRenderer.gameObject.layer = layer;
		if (shape == CanvasShape.Flat)
		{
			Transform obj = _meshRenderer.transform;
			Vector3 localPosition = (_overlay.transform.localPosition = Vector3.zero);
			obj.localPosition = localPosition;
			_meshRenderer.transform.localScale = new Vector3(width, height, 1f);
			_overlay.transform.localScale = new Vector3(x, num8, 1f);
		}
		else
		{
			Transform obj2 = _meshRenderer.transform;
			Vector3 localPosition = (_overlay.transform.localPosition = new Vector3(0f, 0f, (0f - curveRadius) / base.transform.lossyScale.z));
			obj2.localPosition = localPosition;
			_meshRenderer.transform.localScale = new Vector3(width, height, curveRadius / base.transform.lossyScale.z);
			_overlay.transform.localScale = new Vector3(x, num8, curveRadius / base.transform.lossyScale.z);
		}
		_overlay.textures[0] = _renderTexture;
		_overlay.currentOverlayShape = ((shape != 0) ? OVROverlay.OverlayShape.Cylinder : OVROverlay.OverlayShape.Quad);
		_overlay.hidden = !IsCanvasPriority;
		_overlay.useExpensiveSuperSample = expensive;
		_overlay.enabled = Application.isPlaying && _overlayEnabled;
		_overlay.useAutomaticFiltering = true;
		_meshGenerator.SetOverlay(_overlay);
		OVROverlayCanvasSettings.Instance.ApplyGlobalSettings();
	}

	private Color CalcImposterColor()
	{
		if (!overlayEnabled || !IsCanvasPriority || overlayType != OVROverlay.OverlayType.Underlay)
		{
			return Color.white;
		}
		return Color.black;
	}

	private void OnDestroy()
	{
		if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(_imposterMaterial);
			UnityEngine.Object.Destroy(_renderTexture);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(_imposterMaterial);
			UnityEngine.Object.DestroyImmediate(_renderTexture);
		}
	}

	private void OnEnable()
	{
		OVROverlayCanvasManager.AddCanvas(this);
		if ((bool)_overlay)
		{
			_meshRenderer.enabled = ShouldShowImposter;
			_overlay.enabled = Application.isPlaying && _overlayEnabled;
		}
	}

	private void OnDisable()
	{
		OVROverlayCanvasManager.RemoveCanvas(this);
		if ((bool)_overlay)
		{
			_overlay.enabled = false;
			_meshRenderer.enabled = false;
		}
	}

	protected virtual bool ShouldRender()
	{
		if (manualRedraw && _frameIsReady)
		{
			if (_dynamicResolution && _redrawResolutionThreshold != int.MaxValue)
			{
				(int, int)? tuple = CalculateScaledResolution();
				if (tuple.HasValue)
				{
					var (num, num2) = tuple.GetValueOrDefault();
					if (num - _lastPixelWidth < _redrawResolutionThreshold)
					{
						return num2 - _lastPixelHeight >= _redrawResolutionThreshold;
					}
					return true;
				}
			}
			return false;
		}
		if (renderInterval > 1 && Time.frameCount % renderInterval != renderIntervalFrameOffset % renderInterval && _frameIsReady)
		{
			return false;
		}
		if (Application.isEditor)
		{
			return true;
		}
		return IsInFrustum();
	}

	private bool IsInFrustum()
	{
		Camera camera = OVRManager.FindMainCamera();
		if (camera != null)
		{
			XRDisplaySubsystem currentDisplaySubsystem = OVRManager.GetCurrentDisplaySubsystem();
			if (currentDisplaySubsystem != null && currentDisplaySubsystem.GetRenderPassCount() > 0)
			{
				for (int i = 0; i < currentDisplaySubsystem.GetRenderPassCount(); i++)
				{
					currentDisplaySubsystem.GetRenderPass(i, out var renderPass);
					currentDisplaySubsystem.GetCullingParameters(camera, renderPass.cullingPassIndex, out var scriptableCullingParameters);
					GeometryUtility.CalculateFrustumPlanes(scriptableCullingParameters.stereoProjectionMatrix * scriptableCullingParameters.stereoViewMatrix, _FrustumPlanes);
					if (GeometryUtility.TestPlanesAABB(_FrustumPlanes, _meshRenderer.bounds))
					{
						return true;
					}
				}
			}
			else if (camera.stereoEnabled)
			{
				for (int j = 0; j < 2; j++)
				{
					Camera.StereoscopicEye eye = (Camera.StereoscopicEye)j;
					GeometryUtility.CalculateFrustumPlanes(camera.GetStereoProjectionMatrix(eye) * camera.GetStereoViewMatrix(eye), _FrustumPlanes);
					if (GeometryUtility.TestPlanesAABB(_FrustumPlanes, _meshRenderer.bounds))
					{
						return true;
					}
				}
			}
			else
			{
				GeometryUtility.CalculateFrustumPlanes(camera.projectionMatrix * camera.worldToCameraMatrix, _FrustumPlanes);
				if (GeometryUtility.TestPlanesAABB(_FrustumPlanes, _meshRenderer.bounds))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	private void Update()
	{
		UpdateOverlaySettings();
		bool flag = ShouldRender();
		_overlay.isDynamic = flag;
		if (flag)
		{
			ApplyViewportScale();
			_frameIsReady = true;
			RenderCamera();
		}
	}

	private void LateUpdate()
	{
		_imposterMaterial.color = CalcImposterColor();
		_imposterMaterial.mainTextureScale = _imposterTextureScale;
		_imposterMaterial.mainTextureOffset = _imposterTextureOffset;
	}

	public float? GetViewPriorityScore()
	{
		int renderedFrameCount = Time.renderedFrameCount;
		if (_lastViewPriorityScore.frameCount != renderedFrameCount)
		{
			_lastViewPriorityScore = (frameCount: renderedFrameCount, score: GetViewPriorityScoreImpl());
		}
		return _lastViewPriorityScore.score;
	}

	private float? GetViewPriorityScoreImpl()
	{
		Camera camera = OVRManager.FindMainCamera();
		if (camera == null)
		{
			return null;
		}
		if (!_overlayEnabled)
		{
			return null;
		}
		rectTransform.GetWorldCorners(_Corners);
		for (int i = 0; i != 4; i++)
		{
			Vector3 vector = camera.WorldToViewportPoint(_Corners[i]);
			vector.x = Mathf.Clamp01(vector.x) - 0.5f;
			vector.y = Mathf.Clamp01(vector.y) - 0.5f;
			vector.z = ((vector.z < 0f) ? float.NaN : 0f);
			_Corners[i] = vector;
		}
		float num = TriangleArea(_Corners[0], _Corners[1], _Corners[2]) + TriangleArea(_Corners[1], _Corners[2], _Corners[3]);
		float value = num / Mathf.Max(((_Corners[0] + _Corners[1] + _Corners[2] + _Corners[3]) * 0.25f).magnitude, 0.01f);
		if (!float.IsNaN(num))
		{
			return value;
		}
		return null;
	}

	private static float TriangleArea(Vector3 a, Vector3 b, Vector3 c)
	{
		return Vector3.Cross(b - a, c - a).magnitude * 0.5f;
	}

	private void OnValidate()
	{
	}

	private Vector3 GetRectTransformScale()
	{
		Vector3 localScale = rectTransform.localScale;
		Vector3 vector = ((rectTransform.parent != null) ? rectTransform.parent.lossyScale : Vector3.one);
		if ((!Mathf.Approximately(vector.x, vector.y) || !Mathf.Approximately(vector.y, vector.z)) && !_nonUniformScaleWarningShown)
		{
			Debug.LogWarning("[OVROverlayCanvas][" + base.name + "] Non Uniform Parent Scale. This will result in unexpected behavior!", this);
			_nonUniformScaleWarningShown = true;
		}
		return new Vector3(vector.x * localScale.x, vector.y * localScale.y, vector.z * localScale.z);
	}

	private Matrix4x4 GetWorldToViewportMatrix(Camera mainCamera)
	{
		XRDisplaySubsystem currentDisplaySubsystem = OVRManager.GetCurrentDisplaySubsystem();
		if (currentDisplaySubsystem != null && currentDisplaySubsystem.GetRenderPassCount() > 0)
		{
			currentDisplaySubsystem.GetRenderPass(0, out var renderPass);
			renderPass.GetRenderParameter(mainCamera, 0, out var renderParameter);
			return renderParameter.projection * mainCamera.worldToCameraMatrix;
		}
		return mainCamera.projectionMatrix * mainCamera.worldToCameraMatrix;
	}

	private (int pixelWidth, int pixelHeight)? CalculateScaledResolution()
	{
		if (!ShouldScaleViewport)
		{
			return (_renderTexture.width, _renderTexture.height);
		}
		if (!IsInFrustum())
		{
			return (32, 32);
		}
		Camera camera = OVRManager.FindMainCamera();
		if (camera == null)
		{
			return null;
		}
		if (!_optimalResolutionInitialized && XRSettings.isDeviceActive)
		{
			_optimalResolutionWidth = (float)XRSettings.eyeTextureWidth * 2f / XRSettings.eyeTextureResolutionScale;
			_optimalResolutionHeight = (float)XRSettings.eyeTextureHeight * 2f / XRSettings.eyeTextureResolutionScale;
			_optimalResolutionInitialized = _optimalResolutionWidth > 0f && _optimalResolutionHeight > 0f;
		}
		rectTransform.GetLocalCorners(_Corners);
		Matrix4x4 localToWorldMatrix = rectTransform.localToWorldMatrix;
		if (shape == CanvasShape.Curved)
		{
			localToWorldMatrix *= CalculateCurveViewBillboardMatrix(camera);
		}
		Matrix4x4 worldToViewportMatrix = GetWorldToViewportMatrix(camera);
		Matrix4x4 matrix4x = Matrix4x4.Scale(new Vector3(0.5f * _optimalResolutionWidth, 0.5f * _optimalResolutionHeight, 0f)) * worldToViewportMatrix * localToWorldMatrix;
		for (int i = 0; i < 4; i++)
		{
			_Corners[i] = matrix4x.MultiplyPoint(_Corners[i]);
		}
		int num = Mathf.RoundToInt(Mathf.Max((_Corners[1] - _Corners[0]).magnitude, (_Corners[3] - _Corners[2]).magnitude));
		int num2 = Mathf.RoundToInt(Mathf.Max((_Corners[2] - _Corners[1]).magnitude, (_Corners[3] - _Corners[0]).magnitude));
		int value = (num + 1) / 2 * 2 * ((!expensive) ? 1 : 2) + 4;
		int value2 = (num2 + 1) / 2 * 2 * ((!expensive) ? 1 : 2) + 4;
		return new ValueTuple<int, int>(item2: Mathf.Clamp(value, 32, _renderTexture.height), item1: Mathf.Clamp(value2, 32, _renderTexture.width));
	}

	private void ApplyViewportScale()
	{
		(int, int)? tuple = CalculateScaledResolution();
		if (tuple.HasValue)
		{
			var (num, num2) = tuple.GetValueOrDefault();
			if (Math.Abs(num2 - _lastPixelHeight) < 4 && Math.Abs(num - _lastPixelWidth) < 4)
			{
				num = _lastPixelWidth;
				num2 = _lastPixelHeight;
			}
			else
			{
				_lastPixelHeight = num2;
				_lastPixelWidth = num;
			}
			int num3 = num2 - 4;
			int num4 = num - 4;
			Vector3 rectTransformScale = GetRectTransformScale();
			float num5 = rectTransform.rect.height * rectTransformScale.y * (float)num2 / (float)num3;
			float num6 = rectTransform.rect.width * rectTransformScale.x * (float)num / (float)num4;
			_camera.orthographicSize = 0.5f * num5;
			_camera.aspect = num6 / num5;
			float num7 = (float)num / (float)_renderTexture.width;
			float num8 = (float)num2 / (float)_renderTexture.height;
			float num9 = (float)num4 / (float)_renderTexture.width;
			float num10 = (float)num3 / (float)_renderTexture.height;
			_camera.rect = new Rect((1f - num7) / 2f, (1f - num8) / 2f, num7, num8);
			Rect rect = new Rect(0.5f - 0.5f * num9, 0.5f - 0.5f * num10, num9, num10);
			Rect rect2 = new Rect(0f, 0f, 1f, 1f);
			_overlay.overrideTextureRectMatrix = true;
			_overlay.SetSrcDestRects(rect, rect, rect2, rect2);
			int num11 = ((!ShouldScaleViewport) ? 8 : 0);
			Vector2 vector = new Vector2(num, num2);
			Vector2 vector2 = new Vector2(1f / (float)num, 1f / (float)num2);
			_imposterTextureOffset = (rect.min * vector + Vector2.one * num11) * vector2;
			_imposterTextureScale = (rect.size * vector - Vector2.one * num11 * 2f) * vector2;
		}
	}

	private void RenderCamera()
	{
		_camera.transform.position = base.transform.position - _camera.transform.forward;
		Rect rect = _camera.rect;
		int num = (int)(rect.width * (float)_renderTexture.width);
		int num2 = (int)(rect.height * (float)_renderTexture.height);
		ScopedCallback scopedCallback = default(ScopedCallback);
		try
		{
			if (GraphicsSettings.defaultRenderPipeline == null)
			{
				_camera.cullingMask = 1 << CanvasRenderLayer;
				int targetLayer = base.gameObject.layer;
				Transform[] transforms = GetComponentsInChildren<Transform>();
				Transform[] array = transforms;
				foreach (Transform transform in array)
				{
					if (transform.gameObject.layer == targetLayer)
					{
						transform.gameObject.layer = CanvasRenderLayer;
					}
				}
				scopedCallback.OnDispose += delegate
				{
					Transform[] array2 = transforms;
					foreach (Transform transform2 in array2)
					{
						if (transform2.gameObject.layer == CanvasRenderLayer)
						{
							transform2.gameObject.layer = targetLayer;
						}
					}
				};
			}
			else
			{
				_camera.cullingMask = 1 << base.gameObject.layer;
			}
			if (_useTempRT && (num < _renderTexture.width || num2 < _renderTexture.height))
			{
				RenderTexture temporary = RenderTexture.GetTemporary(new RenderTextureDescriptor(num, num2, GraphicsFormat.R8G8B8A8_SRGB, GraphicsFormat.D24_UNorm_S8_UInt, 0));
				temporary.Create();
				_camera.targetTexture = temporary;
				_camera.rect = new Rect(0f, 0f, 1f, 1f);
				_camera.Render();
				Graphics.CopyTexture(temporary, 0, 0, 0, 0, num, num2, _renderTexture, 0, 0, (int)(rect.x * (float)_renderTexture.width), (int)(rect.y * (float)_renderTexture.height));
				RenderTexture.ReleaseTemporary(temporary);
				_camera.rect = rect;
				_camera.targetTexture = _renderTexture;
			}
			else
			{
				_camera.Render();
			}
		}
		finally
		{
			((IDisposable)scopedCallback).Dispose();
		}
	}

	private Matrix4x4 CalculateCurveViewBillboardMatrix(Camera mainCamera)
	{
		Vector3 vector = Quaternion.Inverse(rectTransform.rotation) * (mainCamera.transform.position - rectTransform.position);
		float value = Mathf.Atan2(0f - vector.x, 0f - vector.z);
		Vector3 rectTransformScale = GetRectTransformScale();
		float num = rectTransform.rect.width * rectTransformScale.x / curveRadius;
		value = Mathf.Clamp(value, -0.5f * num, 0.5f * num);
		Vector3 vector2 = new Vector3(value * curveRadius, 0f, 0f);
		Vector3 vector3 = new Vector3(0f, 0f, curveRadius);
		return Matrix4x4.Scale(new Vector3(1f / rectTransformScale.x, 1f / rectTransformScale.y, 1f / rectTransformScale.z)) * Matrix4x4.Translate(-vector3) * Matrix4x4.Rotate(Quaternion.AngleAxis(57.29578f * value, Vector3.up)) * Matrix4x4.Translate(vector3 - vector2) * Matrix4x4.Scale(new Vector3(rectTransformScale.x, rectTransformScale.y, 1f));
	}

	public override Ray TransformRay(Ray ray)
	{
		if (shape != CanvasShape.Curved)
		{
			return ray;
		}
		Vector3 vector = base.transform.InverseTransformPoint(ray.origin);
		Vector3 vector2 = base.transform.InverseTransformDirection(ray.direction);
		float num = curveRadius / base.transform.lossyScale.z;
		Vector3 vector3 = new Vector3(0f, 0f, 0f - num);
		if (!LineCircleIntersection(new Vector2(vector.x, vector.z), new Vector2(vector2.x, vector2.z), new Vector2(vector3.x, vector3.z), num, out var distance))
		{
			return new Ray(ray.origin, base.transform.right);
		}
		Vector3 vector4 = vector + vector2 * distance;
		float x = Mathf.Atan2(vector4.x, vector4.z + num) * num;
		float y = vector4.y;
		return new Ray(base.transform.TransformPoint(new Vector3(x, y, -1f)), base.transform.forward);
	}

	private static bool LineCircleIntersection(Vector2 p1, Vector2 dp, Vector2 center, float radius, out float distance)
	{
		float sqrMagnitude = dp.sqrMagnitude;
		float num = 2f * Vector2.Dot(dp, p1 - center);
		float sqrMagnitude2 = center.sqrMagnitude;
		sqrMagnitude2 += p1.sqrMagnitude;
		sqrMagnitude2 -= 2f * Vector2.Dot(center, p1);
		sqrMagnitude2 -= radius * radius;
		float num2 = num * num - 4f * sqrMagnitude * sqrMagnitude2;
		if (Mathf.Abs(sqrMagnitude) < float.Epsilon || num2 < 0f)
		{
			distance = 0f;
			return false;
		}
		float num3 = (0f - num - Mathf.Sqrt(num2)) / (2f * sqrMagnitude);
		float num4 = (0f - num + Mathf.Sqrt(num2)) / (2f * sqrMagnitude);
		distance = ((num3 >= 0f) ? num3 : num4);
		return true;
	}

	public void SetFrameDirty()
	{
		_frameIsReady = false;
	}

	public void SetCanvasLayer(int layer, bool forceUpdate)
	{
		SetLayerRecursive(base.gameObject, layer, base.gameObject.layer, forceUpdate);
	}

	private static void SetLayerRecursive(GameObject gameObject, int layer, int previousLayer, bool forceUpdate)
	{
		if (gameObject.layer == previousLayer || forceUpdate)
		{
			gameObject.layer = layer;
		}
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			GameObject gameObject2 = gameObject.transform.GetChild(i).gameObject;
			if ((gameObject2.hideFlags &= HideFlags.DontSave) == HideFlags.None)
			{
				SetLayerRecursive(gameObject2, layer, previousLayer, forceUpdate);
			}
		}
	}
}
