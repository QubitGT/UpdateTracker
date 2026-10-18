using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

[ExecuteAlways]
public class MaterialPropertyBlockEditor : MonoBehaviour
{
	[SerializeField]
	private List<Renderer> _renderers;

	[SerializeField]
	private List<MaterialPropertyVector> _vectorProperties;

	[SerializeField]
	private List<MaterialPropertyColor> _colorProperties;

	[SerializeField]
	private List<MaterialPropertyFloat> _floatProperties;

	[SerializeField]
	private bool _updateEveryFrame = true;

	private MaterialPropertyBlock _materialPropertyBlock;

	public List<Renderer> Renderers
	{
		get
		{
			return _renderers;
		}
		set
		{
			_renderers = value;
		}
	}

	public List<MaterialPropertyVector> VectorProperties
	{
		get
		{
			return _vectorProperties;
		}
		set
		{
			_vectorProperties = value;
		}
	}

	public List<MaterialPropertyColor> ColorProperties
	{
		get
		{
			return _colorProperties;
		}
		set
		{
			_colorProperties = value;
		}
	}

	public List<MaterialPropertyFloat> FloatProperties
	{
		get
		{
			return _floatProperties;
		}
		set
		{
			_floatProperties = value;
		}
	}

	public MaterialPropertyBlock MaterialPropertyBlock
	{
		get
		{
			if (_materialPropertyBlock == null)
			{
				_materialPropertyBlock = new MaterialPropertyBlock();
			}
			return _materialPropertyBlock;
		}
	}

	protected virtual void Awake()
	{
		if (_renderers == null)
		{
			Renderer component = GetComponent<Renderer>();
			if (component != null)
			{
				_renderers = new List<Renderer> { component };
			}
		}
		UpdateMaterialPropertyBlock();
	}

	public void UpdateMaterialPropertyBlock()
	{
		MaterialPropertyBlock materialPropertyBlock = MaterialPropertyBlock;
		if (_vectorProperties != null)
		{
			foreach (MaterialPropertyVector vectorProperty in _vectorProperties)
			{
				_materialPropertyBlock.SetVector(vectorProperty.name, vectorProperty.value);
			}
		}
		if (_colorProperties != null)
		{
			foreach (MaterialPropertyColor colorProperty in _colorProperties)
			{
				_materialPropertyBlock.SetColor(colorProperty.name, colorProperty.value);
			}
		}
		if (_floatProperties != null)
		{
			foreach (MaterialPropertyFloat floatProperty in _floatProperties)
			{
				_materialPropertyBlock.SetFloat(floatProperty.name, floatProperty.value);
			}
		}
		if (_renderers == null)
		{
			return;
		}
		foreach (Renderer renderer in _renderers)
		{
			renderer.SetPropertyBlock(materialPropertyBlock);
		}
	}

	protected virtual void Update()
	{
		if (_updateEveryFrame)
		{
			UpdateMaterialPropertyBlock();
		}
	}
}
