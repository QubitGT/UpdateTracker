using System;
using UnityEngine;

namespace Oculus.Interaction.UnityCanvas;

[DisallowMultipleComponent]
public class CanvasRenderTexture : MonoBehaviour
{
	private class TransformChangeListener : MonoBehaviour
	{
		public event Action WhenRectTransformDimensionsChanged = delegate
		{
		};

		private void OnRectTransformDimensionsChange()
		{
			this.WhenRectTransformDimensionsChanged();
		}
	}

	public enum DriveMode
	{
		Auto,
		Manual
	}

	public static class Properties
	{
		public static readonly string DimensionDriveMode = "_dimensionsDriveMode";

		public static readonly string Resolution = "_resolution";

		public static readonly string RenderScale = "_renderScale";

		public static readonly string PixelsPerUnit = "_pixelsPerUnit";

		public static readonly string RenderLayers = "_renderingLayers";

		public static readonly string GenerateMipMaps = "_generateMipMaps";

		public static readonly string Canvas = "_canvas";
	}

	public const int DEFAULT_UI_LAYERMASK = 32;

	private static readonly Vector2Int DEFAULT_TEXTURE_RES = new Vector2Int(128, 128);

	[Tooltip("The Unity canvas that will be rendered.")]
	[SerializeField]
	private Canvas _canvas;

	[Tooltip("Used to increase resolution of rendered canvas. If you need extra resolution, you can use this as a whole-integer multiplier of the final resolution used to render the texture.")]
	[Range(1f, 3f)]
	[Delayed]
	[SerializeField]
	private int _renderScale = 1;

	[Tooltip("If set to auto, texture dimensions will take the size of the attached RectTransform into consideration, in addition to the configured pixel-per-unit ratio.")]
	[SerializeField]
	private DriveMode _dimensionsDriveMode;

	[Tooltip("The exact pixel resolution of the texture used for interface rendering.")]
	[Delayed]
	[SerializeField]
	private Vector2Int _resolution = DEFAULT_TEXTURE_RES;

	[Tooltip("Whether or not mip-maps should be auto-generated for the texture. Can help aliasing if the texture can be viewed from many difference distances.")]
	[SerializeField]
	private bool _generateMipMaps;

	[Tooltip("Pixels per unit ratio used to drive the texture dimensions. Determines the RenderTexture size from the canvas world size.")]
	[SerializeField]
	private int _pixelsPerUnit = 100;

	[Header("Rendering Settings")]
	[Tooltip("The layers to render when the rendering texture is created. All child renderers should be part of this mask.")]
	[SerializeField]
	private LayerMask _renderingLayers = 32;

	public Action<Texture> OnUpdateRenderTexture = delegate
	{
	};

	private TransformChangeListener _listener;

	private RenderTexture _tex;

	private Camera _camera;

	protected bool _started;

	public LayerMask RenderingLayers => _renderingLayers;

	public int RenderScale
	{
		get
		{
			return _renderScale;
		}
		set
		{
			if (_renderScale < 1 || _renderScale > 3)
			{
				throw new ArgumentException($"Render scale must be between 1 and 3, but was {value}");
			}
			if (_renderScale != value)
			{
				_renderScale = value;
				if (base.isActiveAndEnabled && Application.isPlaying)
				{
					UpdateCamera();
				}
			}
		}
	}

	public Camera OverlayCamera => _camera;

	public Texture Texture => _tex;

	public Vector2Int CalcAutoResolution()
	{
		if (_canvas == null)
		{
			return DEFAULT_TEXTURE_RES;
		}
		RectTransform component = _canvas.GetComponent<RectTransform>();
		if (component == null)
		{
			return DEFAULT_TEXTURE_RES;
		}
		Vector2 sizeDelta = component.sizeDelta;
		sizeDelta.x *= component.lossyScale.x;
		sizeDelta.y *= component.lossyScale.y;
		int a = Mathf.RoundToInt(UnitsToPixels(sizeDelta.x));
		return new Vector2Int(y: Mathf.Max(Mathf.RoundToInt(UnitsToPixels(sizeDelta.y)), 1), x: Mathf.Max(a, 1));
	}

	public Vector2Int GetBaseResolutionToUse()
	{
		if (_dimensionsDriveMode == DriveMode.Auto)
		{
			return CalcAutoResolution();
		}
		return _resolution;
	}

	public Vector2Int GetScaledResolutionToUse()
	{
		return Vector2Int.RoundToInt((Vector2)GetBaseResolutionToUse() * (float)_renderScale);
	}

	public float PixelsToUnits(float pixels)
	{
		return 1f / (float)_pixelsPerUnit * pixels;
	}

	public float UnitsToPixels(float units)
	{
		return (float)_pixelsPerUnit * units;
	}

	protected void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected void OnEnable()
	{
		if (_started)
		{
			if (_listener == null)
			{
				_listener = _canvas.gameObject.AddComponent<TransformChangeListener>();
			}
			_listener.WhenRectTransformDimensionsChanged += WhenCanvasRectTransformDimensionsChanged;
			UpdateCamera();
		}
	}

	private void WhenCanvasRectTransformDimensionsChanged()
	{
		UpdateCamera();
	}

	protected void OnDisable()
	{
		if (_started)
		{
			if (_camera?.gameObject != null)
			{
				UnityEngine.Object.Destroy(_camera.gameObject);
			}
			if (_tex != null)
			{
				UnityEngine.Object.DestroyImmediate(_tex);
			}
			if (_listener != null)
			{
				_listener.WhenRectTransformDimensionsChanged -= WhenCanvasRectTransformDimensionsChanged;
			}
		}
	}

	protected void UpdateCamera()
	{
		if (!Application.isPlaying || !_started)
		{
			return;
		}
		try
		{
			if (_camera == null)
			{
				GameObject gameObject = CreateChildObject("__Camera");
				_camera = gameObject.AddComponent<Camera>();
				_camera.orthographic = true;
				_camera.nearClipPlane = -0.1f;
				_camera.farClipPlane = 0.1f;
				_camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
				_camera.clearFlags = CameraClearFlags.Color;
			}
			UpdateRenderTexture();
			UpdateOrthoSize();
			UpdateCameraCullingMask();
		}
		finally
		{
		}
	}

	protected void UpdateRenderTexture()
	{
		try
		{
			Vector2Int scaledResolutionToUse = GetScaledResolutionToUse();
			if (_tex == null || _tex.width != scaledResolutionToUse.x || _tex.height != scaledResolutionToUse.y || _tex.autoGenerateMips != _generateMipMaps)
			{
				if (_tex != null)
				{
					_camera.targetTexture = null;
					UnityEngine.Object.DestroyImmediate(_tex);
				}
				_tex = new RenderTexture(scaledResolutionToUse.x, scaledResolutionToUse.y, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
				_tex.filterMode = FilterMode.Bilinear;
				_tex.autoGenerateMips = _generateMipMaps;
				_camera.targetTexture = _tex;
				OnUpdateRenderTexture(_tex);
			}
		}
		finally
		{
		}
	}

	private void UpdateOrthoSize()
	{
		if (_camera != null)
		{
			_camera.orthographicSize = PixelsToUnits(GetBaseResolutionToUse().y) * 0.5f;
		}
	}

	private void UpdateCameraCullingMask()
	{
		if (_camera != null)
		{
			_camera.cullingMask = _renderingLayers.value;
		}
	}

	protected GameObject CreateChildObject(string name)
	{
		GameObject obj = new GameObject(name);
		obj.transform.SetParent(_canvas.transform);
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale = Vector3.one;
		return obj;
	}

	public void InjectAllCanvasRenderTexture(Canvas canvas, int pixelsPerUnit, int renderScale, LayerMask renderingLayers, bool generateMipMaps)
	{
		InjectCanvas(canvas);
		InjectPixelsPerUnit(pixelsPerUnit);
		InjectRenderScale(renderScale);
		InjectRenderingLayers(renderingLayers);
		InjectGenerateMipMaps(generateMipMaps);
	}

	public void InjectCanvas(Canvas canvas)
	{
		_canvas = canvas;
	}

	public void InjectPixelsPerUnit(int pixelsPerUnit)
	{
		_pixelsPerUnit = pixelsPerUnit;
	}

	public void InjectRenderScale(int renderScale)
	{
		_renderScale = renderScale;
	}

	public void InjectRenderingLayers(LayerMask renderingLayers)
	{
		_renderingLayers = renderingLayers;
	}

	public void InjectGenerateMipMaps(bool generateMipMaps)
	{
		_generateMipMaps = generateMipMaps;
	}
}
