using System;
using UnityEngine;

namespace Oculus.Interaction.UnityCanvas;

public class CanvasMeshRenderer : MonoBehaviour
{
	public static class Properties
	{
		public static readonly string RenderingMode = "_renderingMode";

		public static readonly string UseAlphaToMask = "_useAlphaToMask";

		public static readonly string AlphaCutoutThreshold = "_alphaCutoutThreshold";
	}

	private static readonly int MainTexShaderID = Shader.PropertyToID("_MainTex");

	[Tooltip("The canvas texture that will be rendered.")]
	[SerializeField]
	protected CanvasRenderTexture _canvasRenderTexture;

	[Tooltip("The mesh renderer that will be driven.")]
	[SerializeField]
	protected MeshRenderer _meshRenderer;

	[Tooltip("Determines the shader used for rendering. For details on these rendering modes, see the Curved Canvas topic in the documentation.")]
	[SerializeField]
	protected int _renderingMode = 1;

	[Tooltip("Requires MSAA. Provides limited transparency useful for anti-aliasing soft edges of UI elements.")]
	[SerializeField]
	private bool _useAlphaToMask = true;

	[Tooltip("Select the alpha cutoff used for the cutout rendering.")]
	[Range(0f, 1f)]
	[SerializeField]
	private float _alphaCutoutThreshold = 0.5f;

	protected Material _material;

	protected bool _started;

	private RenderingMode RenderingMode => (RenderingMode)_renderingMode;

	protected virtual string GetShaderName()
	{
		switch (RenderingMode)
		{
		case RenderingMode.AlphaBlended:
			return "Hidden/Imposter_AlphaBlended";
		case RenderingMode.AlphaCutout:
			if (_useAlphaToMask)
			{
				return "Hidden/Imposter_AlphaToMask";
			}
			return "Hidden/Imposter_AlphaCutout";
		default:
			return "Hidden/Imposter_Opaque";
		}
	}

	protected virtual void SetAdditionalProperties(MaterialPropertyBlock block)
	{
		block.SetFloat("_Cutoff", GetAlphaCutoutThreshold());
	}

	protected virtual float GetAlphaCutoutThreshold()
	{
		if (RenderingMode == RenderingMode.AlphaCutout && !_useAlphaToMask)
		{
			return _alphaCutoutThreshold;
		}
		return 1f;
	}

	protected virtual void HandleUpdateRenderTexture(Texture texture)
	{
		_meshRenderer.material = _material;
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		_meshRenderer.GetPropertyBlock(materialPropertyBlock);
		materialPropertyBlock.SetTexture(MainTexShaderID, texture);
		SetAdditionalProperties(materialPropertyBlock);
		_meshRenderer.SetPropertyBlock(materialPropertyBlock);
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			try
			{
				_material = new Material(Shader.Find(GetShaderName()));
			}
			finally
			{
			}
			CanvasRenderTexture canvasRenderTexture = _canvasRenderTexture;
			canvasRenderTexture.OnUpdateRenderTexture = (Action<Texture>)Delegate.Combine(canvasRenderTexture.OnUpdateRenderTexture, new Action<Texture>(HandleUpdateRenderTexture));
			if (_canvasRenderTexture.Texture != null)
			{
				HandleUpdateRenderTexture(_canvasRenderTexture.Texture);
			}
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			if (_material != null)
			{
				UnityEngine.Object.Destroy(_material);
				_material = null;
			}
			CanvasRenderTexture canvasRenderTexture = _canvasRenderTexture;
			canvasRenderTexture.OnUpdateRenderTexture = (Action<Texture>)Delegate.Remove(canvasRenderTexture.OnUpdateRenderTexture, new Action<Texture>(HandleUpdateRenderTexture));
		}
	}

	public void InjectAllCanvasMeshRenderer(CanvasRenderTexture canvasRenderTexture, MeshRenderer meshRenderer)
	{
		InjectCanvasRenderTexture(canvasRenderTexture);
		InjectMeshRenderer(meshRenderer);
	}

	public void InjectCanvasRenderTexture(CanvasRenderTexture canvasRenderTexture)
	{
		_canvasRenderTexture = canvasRenderTexture;
	}

	public void InjectMeshRenderer(MeshRenderer meshRenderer)
	{
		_meshRenderer = meshRenderer;
	}

	public void InjectOptionalRenderingMode(RenderingMode renderingMode)
	{
		_renderingMode = (int)renderingMode;
	}

	public void InjectOptionalAlphaCutoutThreshold(float alphaCutoutThreshold)
	{
		_alphaCutoutThreshold = alphaCutoutThreshold;
	}

	public void InjectOptionalUseAlphaToMask(bool useAlphaToMask)
	{
		_useAlphaToMask = useAlphaToMask;
	}
}
