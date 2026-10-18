using UnityEngine;

namespace Oculus.Interaction.Samples;

public class OpacityFromAnimatedTransformController : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The renderer to which the opacity should be applied")]
	private Renderer _renderer;

	[SerializeField]
	[Tooltip("The animation-controlled transform whose X magnitude will be applied to the renderer as `_Opacity`")]
	private Transform _opacityTransform;

	private MaterialPropertyBlock _materialProperties;

	private bool _isSkinnedMeshRenderer;

	private void Start()
	{
		_isSkinnedMeshRenderer = _renderer is SkinnedMeshRenderer;
		if (!_isSkinnedMeshRenderer)
		{
			_materialProperties = new MaterialPropertyBlock();
		}
	}

	private void Update()
	{
		float value = Mathf.Abs(_opacityTransform.localPosition.x);
		if (_isSkinnedMeshRenderer)
		{
			_renderer.material.SetFloat("_Opacity", value);
			return;
		}
		_materialProperties.SetFloat("_Opacity", value);
		_renderer.SetPropertyBlock(_materialProperties);
	}
}
