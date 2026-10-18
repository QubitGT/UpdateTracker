using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

[RequireComponent(typeof(Canvas))]
public sealed class OverlayCanvas : MonoBehaviour
{
	private static Plane[] FrustumPlanes = new Plane[6];

	private Camera _camera;

	private OVROverlay _overlay;

	private RenderTexture _renderTexture;

	private MeshRenderer _meshRenderer;

	private Mesh _quad;

	private Material _defaultMat;

	private const int MaxTextureSize = 1600;

	private const int MinTextureSize = 200;

	private const float PixelsPerUnit = 1f;

	private readonly bool _scaleViewport = Application.isMobilePlatform;

	public OverlayCanvasPanel Panel { get; set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		FrustumPlanes = new Plane[6];
	}

	private void Start()
	{
		Transform transform = Panel.Transform;
		RectTransform rectTransform = Panel.RectTransform;
		Rect rect = rectTransform.rect;
		float width = rect.width;
		float height = rect.height;
		float num = ((width >= height) ? 1f : (width / height));
		float num2 = ((height >= width) ? 1f : (height / width));
		int num3 = ((!_scaleViewport) ? 8 : 0);
		int num4 = Mathf.CeilToInt(num * (float)(1600 - num3 * 2));
		int num5 = Mathf.CeilToInt(num2 * (float)(1600 - num3 * 2));
		int num6 = num4 + num3 * 2;
		int num7 = num5 + num3 * 2;
		float x = width * ((float)num6 / (float)num4);
		float num8 = height * ((float)num7 / (float)num5);
		float num9 = (float)num4 / (float)num6;
		float num10 = (float)num5 / (float)num7;
		_renderTexture = new RenderTexture(num6, num7, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default)
		{
			useMipMap = !_scaleViewport
		};
		GameObject gameObject = new GameObject(base.name + " Overlay Camera");
		gameObject.transform.SetParent(transform, worldPositionStays: false);
		_camera = gameObject.AddComponent<Camera>();
		_camera.stereoTargetEye = StereoTargetEyeMask.None;
		_camera.transform.position = transform.position - transform.forward;
		_camera.orthographic = true;
		_camera.enabled = false;
		_camera.targetTexture = _renderTexture;
		_camera.cullingMask = 1 << base.gameObject.layer;
		_camera.clearFlags = CameraClearFlags.Color;
		_camera.backgroundColor = Color.clear;
		_camera.orthographicSize = 0.5f * num8 * rectTransform.localScale.y;
		_camera.nearClipPlane = 0.99f;
		_camera.farClipPlane = 1.01f;
		_quad = new Mesh
		{
			name = base.name + " Overlay Quad",
			vertices = new Vector3[4]
			{
				new Vector3(-0.5f, -0.5f),
				new Vector3(-0.5f, 0.5f),
				new Vector3(0.5f, 0.5f),
				new Vector3(0.5f, -0.5f)
			},
			uv = new Vector2[4]
			{
				new Vector2(0f, 0f),
				new Vector2(0f, 1f),
				new Vector2(1f, 1f),
				new Vector2(1f, 0f)
			},
			triangles = new int[6] { 0, 1, 2, 2, 3, 0 },
			bounds = new Bounds(Vector3.zero, Vector3.one)
		};
		_quad.UploadMeshData(markNoLongerReadable: true);
		Shader shader = Shader.Find("UI/IDF Prerendered");
		_defaultMat = new Material(shader)
		{
			mainTexture = _renderTexture,
			color = Color.black,
			mainTextureOffset = new Vector2(0.5f - 0.5f * num9, 0.5f - 0.5f * num10),
			mainTextureScale = new Vector2(num9, num10)
		};
		GameObject gameObject2 = new GameObject(base.name + " MeshRenderer");
		gameObject2.transform.SetParent(base.transform, worldPositionStays: false);
		gameObject2.AddComponent<MeshFilter>().sharedMesh = _quad;
		_meshRenderer = gameObject2.AddComponent<MeshRenderer>();
		_meshRenderer.sharedMaterial = _defaultMat;
		gameObject2.layer = RuntimeSettings.Instance.MeshRendererLayer;
		gameObject2.transform.localScale = new Vector3(width, height, 1f);
		GameObject gameObject3 = new GameObject(base.name + " Overlay");
		gameObject3.transform.SetParent(base.transform, worldPositionStays: false);
		_overlay = gameObject3.AddComponent<OVROverlay>();
		_overlay.isDynamic = true;
		_overlay.isAlphaPremultiplied = !Application.isMobilePlatform;
		_overlay.textures[0] = _renderTexture;
		_overlay.currentOverlayType = OVROverlay.OverlayType.Overlay;
		_overlay.compositionDepth = RuntimeSettings.Instance.OverlayDepth;
		_overlay.noDepthBufferTesting = true;
		_overlay.transform.localScale = new Vector3(x, num8, 1f);
		_overlay.currentOverlayShape = OVROverlay.OverlayShape.Cylinder;
		gameObject3.transform.SetParent(Panel.Interface.Transform, worldPositionStays: false);
	}

	private void OnDestroy()
	{
		Object.Destroy(_defaultMat);
		Object.Destroy(_quad);
		Object.Destroy(_renderTexture);
	}

	private void OnEnable()
	{
		if ((bool)_meshRenderer)
		{
			_meshRenderer.enabled = true;
		}
		if ((bool)_overlay)
		{
			_overlay.enabled = true;
		}
		if ((bool)_camera)
		{
			_camera.enabled = true;
		}
	}

	private void OnDisable()
	{
		if ((bool)_meshRenderer)
		{
			_meshRenderer.enabled = false;
		}
		if ((bool)_overlay)
		{
			_overlay.enabled = false;
		}
		if ((bool)_camera)
		{
			_camera.enabled = false;
		}
	}

	private bool ShouldRender(Camera baseCamera)
	{
		if (baseCamera == null)
		{
			return false;
		}
		for (int i = 0; i < 2; i++)
		{
			Camera.StereoscopicEye eye = (Camera.StereoscopicEye)i;
			GeometryUtility.CalculateFrustumPlanes(baseCamera.GetStereoProjectionMatrix(eye) * baseCamera.GetStereoViewMatrix(eye), FrustumPlanes);
			if (GeometryUtility.TestPlanesAABB(FrustumPlanes, _meshRenderer.bounds))
			{
				return true;
			}
		}
		return false;
	}

	private void Update()
	{
		Camera main = Camera.main;
		if (ShouldRender(main))
		{
			Transform transform = Panel.Transform;
			RectTransform rectTransform = Panel.RectTransform;
			if (_scaleViewport)
			{
				Rect rect = rectTransform.rect;
				float magnitude = (main.transform.position - base.transform.position).magnitude;
				float value = Mathf.Ceil(1f * Mathf.Max(rect.width * transform.lossyScale.x, rect.height * transform.lossyScale.y) / magnitude / 8f * (float)_renderTexture.height) * 8f;
				value = Mathf.Clamp(value, 200f, _renderTexture.height);
				float num = value - 2f;
				_camera.orthographicSize = 0.5f * rect.height * rectTransform.localScale.y * value / num;
				float num2 = rect.width / rect.height;
				float num3 = num * num2;
				float num4 = Mathf.Ceil((num3 + 2f) * 0.5f) * 2f / (float)_renderTexture.width;
				float num5 = value / (float)_renderTexture.height;
				float num6 = num3 / (float)_renderTexture.width;
				float num7 = num / (float)_renderTexture.height;
				_camera.rect = new Rect((1f - num4) / 2f, (1f - num5) / 2f, num4, num5);
				Rect rect2 = new Rect(0.5f - 0.5f * num6, 0.5f - 0.5f * num7, num6, num7);
				_defaultMat.mainTextureOffset = rect2.min;
				_defaultMat.mainTextureScale = rect2.size;
				_overlay.overrideTextureRectMatrix = true;
				rect2.y = 1f - rect2.height - rect2.y;
				Rect rect3 = new Rect(0f, 0f, 1f, 1f);
				_overlay.SetSrcDestRects(rect2, rect2, rect3, rect3);
			}
			_camera.Render();
			Transform obj = _overlay.transform;
			obj.localPosition = Vector3.zero;
			obj.localRotation = transform.localRotation;
			Vector2 vector = rectTransform.sizeDelta / Panel.PixelsPerUnit;
			obj.localScale = new Vector3(vector.x, vector.y, Panel.SphericalCoordinates.x);
		}
	}
}
