using Meta.XR.Util;
using UnityEngine;

namespace Oculus.Interaction.UnityCanvas;

[Feature(Feature.Interaction)]
public class OVRCanvasMeshRenderer : CanvasMeshRenderer
{
	public new static class Properties
	{
		public static readonly string CanvasRenderTexture = "_canvasRenderTexture";

		public static readonly string CanvasMesh = "_canvasMesh";

		public static readonly string EnableSuperSampling = "_enableSuperSampling";

		public static readonly string EmulateWhileInEditor = "_emulateWhileInEditor";

		public static readonly string DoUnderlayAntiAliasing = "_doUnderlayAntiAliasing";

		public static readonly string RuntimeOffset = "_runtimeOffset";
	}

	[SerializeField]
	protected CanvasMesh _canvasMesh;

	[Tooltip("If non-zero it will cause the position of the overlay to be offset by this amount at runtime, while the renderer will remain where it was at edit time. This can be used to prevent the two representations from overlapping.")]
	[SerializeField]
	protected Vector3 _runtimeOffset = new Vector3(0f, 0f, 0f);

	[Tooltip("Uses a more expensive image sampling technique for improved quality at the cost of performance.")]
	[SerializeField]
	protected bool _enableSuperSampling = true;

	[Tooltip("Attempts to anti-alias the edges of the underlay by using alpha blending.  Can cause borders of darkness around partially transparent objects.")]
	[SerializeField]
	private bool _doUnderlayAntiAliasing;

	[Tooltip("OVR Layers can provide a buggy or less ideal workflow while in the editor.  This option allows you emulate the layer rendering while in the editor, while still using the OVR Layer rendering in a build.")]
	[SerializeField]
	private bool _emulateWhileInEditor = true;

	protected OVROverlay _overlay;

	private OVRRenderingMode RenderingMode => (OVRRenderingMode)_renderingMode;

	public bool ShouldUseOVROverlay
	{
		get
		{
			OVRRenderingMode renderingMode = RenderingMode;
			if ((uint)(renderingMode - 100) <= 1u)
			{
				return !UseEditorEmulation();
			}
			return false;
		}
	}

	protected override string GetShaderName()
	{
		switch (RenderingMode)
		{
		case OVRRenderingMode.Overlay:
			return "Hidden/Imposter_AlphaCutout";
		case OVRRenderingMode.Underlay:
			if (UseEditorEmulation())
			{
				return "Hidden/Imposter_AlphaCutout";
			}
			if (_doUnderlayAntiAliasing)
			{
				return "Hidden/Imposter_Underlay_AA";
			}
			return "Hidden/Imposter_Underlay";
		default:
			return base.GetShaderName();
		}
	}

	protected override float GetAlphaCutoutThreshold()
	{
		switch (RenderingMode)
		{
		case OVRRenderingMode.Overlay:
			return 1f;
		case OVRRenderingMode.Underlay:
			if (!UseEditorEmulation())
			{
				return 1f;
			}
			return 0.5f;
		default:
			return base.GetAlphaCutoutThreshold();
		}
	}

	protected override void HandleUpdateRenderTexture(Texture texture)
	{
		base.HandleUpdateRenderTexture(texture);
		UpdateOverlay(texture);
	}

	private bool UseEditorEmulation()
	{
		if (!Application.isEditor)
		{
			return false;
		}
		return _emulateWhileInEditor;
	}

	private bool GetOverlayParameters(out OVROverlay.OverlayShape shape, out Vector3 position, out Vector3 scale)
	{
		if (_canvasMesh is CanvasCylinder canvasCylinder)
		{
			shape = OVROverlay.OverlayShape.Cylinder;
			Vector2Int baseResolutionToUse = _canvasRenderTexture.GetBaseResolutionToUse();
			position = new Vector3(0f, 0f, 0f - canvasCylinder.Radius) - _runtimeOffset;
			scale = new Vector3(_canvasRenderTexture.PixelsToUnits(baseResolutionToUse.x) / canvasCylinder.transform.lossyScale.x, _canvasRenderTexture.PixelsToUnits(baseResolutionToUse.y) / canvasCylinder.transform.lossyScale.y, canvasCylinder.Radius);
			return true;
		}
		if (_canvasMesh is CanvasRect)
		{
			shape = OVROverlay.OverlayShape.Quad;
			Vector2Int baseResolutionToUse2 = _canvasRenderTexture.GetBaseResolutionToUse();
			position = -_runtimeOffset;
			scale = new Vector3(_canvasRenderTexture.PixelsToUnits(baseResolutionToUse2.x), _canvasRenderTexture.PixelsToUnits(baseResolutionToUse2.y), 1f);
			return true;
		}
		shape = OVROverlay.OverlayShape.Quad;
		position = Vector3.zero;
		scale = Vector3.zero;
		return false;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		this.EndStart(ref _started);
	}

	protected void UpdateOverlay(Texture texture)
	{
		try
		{
			if (!ShouldUseOVROverlay)
			{
				_overlay?.gameObject?.SetActive(value: false);
				return;
			}
			if (_overlay == null)
			{
				GameObject gameObject = CreateChildObject("__Overlay");
				_overlay = gameObject.AddComponent<OVROverlay>();
				_overlay.isAlphaPremultiplied = !Application.isMobilePlatform;
			}
			else
			{
				_overlay.gameObject.SetActive(value: true);
			}
			if (!GetOverlayParameters(out var shape, out var position, out var scale))
			{
				_overlay.gameObject.SetActive(value: false);
				return;
			}
			bool flag = RenderingMode == OVRRenderingMode.Underlay;
			_overlay.textures = new Texture[1] { texture };
			_overlay.noDepthBufferTesting = flag;
			_overlay.currentOverlayType = (flag ? OVROverlay.OverlayType.Underlay : OVROverlay.OverlayType.Overlay);
			_overlay.currentOverlayShape = shape;
			_overlay.useExpensiveSuperSample = _enableSuperSampling;
			_overlay.transform.localPosition = position;
			_overlay.transform.localScale = scale;
		}
		finally
		{
		}
	}

	protected GameObject CreateChildObject(string name)
	{
		GameObject obj = new GameObject(name);
		obj.transform.SetParent(base.transform);
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale = Vector3.one;
		return obj;
	}

	public void InjectAllOVRCanvasMeshRenderer(CanvasRenderTexture canvasRenderTexture, MeshRenderer meshRenderer, CanvasMesh canvasMesh)
	{
		InjectAllCanvasMeshRenderer(canvasRenderTexture, meshRenderer);
		InjectCanvasMesh(canvasMesh);
	}

	public void InjectCanvasMesh(CanvasMesh canvasMesh)
	{
		_canvasMesh = canvasMesh;
	}

	public void InjectOptionalRenderingMode(OVRRenderingMode ovrRenderingMode)
	{
		_renderingMode = (int)ovrRenderingMode;
	}

	public void InjectOptionalDoUnderlayAntiAliasing(bool doUnderlayAntiAliasing)
	{
		_doUnderlayAntiAliasing = doUnderlayAntiAliasing;
	}

	public void InjectOptionalEnableSuperSampling(bool enableSuperSampling)
	{
		_enableSuperSampling = enableSuperSampling;
	}
}
