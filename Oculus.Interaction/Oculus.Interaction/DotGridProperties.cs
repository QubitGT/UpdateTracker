using UnityEngine;

namespace Oculus.Interaction;

[ExecuteAlways]
public class DotGridProperties : MonoBehaviour
{
	[SerializeField]
	private MaterialPropertyBlockEditor _materialPropertyBlockEditor;

	[SerializeField]
	private int _columns;

	[SerializeField]
	private int _rows;

	[SerializeField]
	private float _radius;

	[SerializeField]
	private Color _color;

	private bool _change;

	private readonly int _colorShaderID = Shader.PropertyToID("_Color");

	private readonly int _dimensionsShaderID = Shader.PropertyToID("_Dimensions");

	public int Columns
	{
		get
		{
			return _columns;
		}
		set
		{
			_columns = value;
		}
	}

	public int Rows
	{
		get
		{
			return _rows;
		}
		set
		{
			_rows = value;
		}
	}

	public float Radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
		}
	}

	public Color Color
	{
		get
		{
			return _color;
		}
		set
		{
			_color = value;
		}
	}

	protected virtual void Start()
	{
		_change = true;
	}

	protected virtual void Update()
	{
		if (_change && !(_materialPropertyBlockEditor == null))
		{
			MaterialPropertyBlock materialPropertyBlock = _materialPropertyBlockEditor.MaterialPropertyBlock;
			materialPropertyBlock.SetColor(_colorShaderID, _color);
			materialPropertyBlock.SetVector(_dimensionsShaderID, new Vector4(_columns, _rows, _radius, 0f));
			_materialPropertyBlockEditor.UpdateMaterialPropertyBlock();
			_change = false;
		}
	}

	protected virtual void OnValidate()
	{
		_change = true;
	}

	public void InjectAllDotGridProperties(MaterialPropertyBlockEditor editor)
	{
		InjectMaterialPropertyBlockEditor(editor);
	}

	public void InjectMaterialPropertyBlockEditor(MaterialPropertyBlockEditor editor)
	{
		_materialPropertyBlockEditor = editor;
	}
}
