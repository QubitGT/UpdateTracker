using UnityEngine;

namespace Oculus.Interaction.Samples;

public class MRPassThroughMaterialChanger : MonoBehaviour
{
	[Header("Passthrough Material")]
	[Tooltip("Material that should be rendered during passthrough")]
	[SerializeField]
	private Material _passThroughMaterial;

	[Header("Current GameObject Material")]
	[SerializeField]
	private Material _material;

	[Tooltip("This current gameobject renderer")]
	[SerializeField]
	private Renderer _renderer;

	protected bool _started;

	protected virtual void Reset()
	{
		_renderer = base.gameObject.GetComponent<Renderer>();
		_material = _renderer.material;
	}

	protected virtual void Start()
	{
		if (_renderer == null)
		{
			_renderer = base.gameObject.GetComponent<Renderer>();
			_material = _renderer.material;
		}
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	private void Update()
	{
		if (_passThroughMaterial != null && MRPassthrough.PassThrough.IsPassThroughOn)
		{
			_renderer.material = _passThroughMaterial;
		}
		else
		{
			_renderer.material = _material;
		}
	}

	public void InjectAllChanger(Material passthroughMaterial, Renderer render, Material material)
	{
		InjectPassthroughMaterial(passthroughMaterial);
		InjectRenderer(render);
		InjectMaterial(material);
	}

	public void InjectPassthroughMaterial(Material passthroughMaterial)
	{
		_passThroughMaterial = passthroughMaterial;
	}

	public void InjectRenderer(Renderer render)
	{
		_renderer = render;
	}

	public void InjectMaterial(Material material)
	{
		_material = material;
	}
}
