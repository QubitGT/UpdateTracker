using UnityEngine;

namespace Oculus.Interaction;

[ExecuteAlways]
public class RoundedBoxProperties : MonoBehaviour
{
	[SerializeField]
	private MaterialPropertyBlockEditor _editor;

	[SerializeField]
	private float _width = 1f;

	[SerializeField]
	private float _height = 1f;

	[SerializeField]
	private Color _color = Color.white;

	[SerializeField]
	private Color _borderColor = Color.black;

	[SerializeField]
	private float _radiusTopLeft;

	[SerializeField]
	private float _radiusTopRight;

	[SerializeField]
	private float _radiusBottomLeft;

	[SerializeField]
	private float _radiusBottomRight;

	[SerializeField]
	private float _borderInnerRadius;

	[SerializeField]
	private float _borderOuterRadius;

	private readonly int _colorShaderID = Shader.PropertyToID("_Color");

	private readonly int _borderColorShaderID = Shader.PropertyToID("_BorderColor");

	private readonly int _radiiShaderID = Shader.PropertyToID("_Radii");

	private readonly int _dimensionsShaderID = Shader.PropertyToID("_Dimensions");

	public float Width
	{
		get
		{
			return _width;
		}
		set
		{
			_width = value;
			UpdateSize();
		}
	}

	public float Height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = value;
			UpdateSize();
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

	public Color BorderColor
	{
		get
		{
			return _borderColor;
		}
		set
		{
			_borderColor = value;
		}
	}

	public float RadiusTopLeft
	{
		get
		{
			return _radiusTopLeft;
		}
		set
		{
			_radiusTopLeft = value;
		}
	}

	public float RadiusTopRight
	{
		get
		{
			return _radiusTopRight;
		}
		set
		{
			_radiusTopRight = value;
		}
	}

	public float RadiusBottomLeft
	{
		get
		{
			return _radiusBottomLeft;
		}
		set
		{
			_radiusBottomLeft = value;
		}
	}

	public float RadiusBottomRight
	{
		get
		{
			return _radiusBottomRight;
		}
		set
		{
			_radiusBottomRight = value;
		}
	}

	public float BorderInnerRadius
	{
		get
		{
			return _borderInnerRadius;
		}
		set
		{
			_borderInnerRadius = value;
		}
	}

	public float BorderOuterRadius
	{
		get
		{
			return _borderOuterRadius;
		}
		set
		{
			_borderOuterRadius = value;
			UpdateSize();
		}
	}

	protected virtual void Awake()
	{
		UpdateSize();
		UpdateMaterialPropertyBlock();
	}

	protected virtual void Start()
	{
		UpdateSize();
		UpdateMaterialPropertyBlock();
	}

	private void UpdateSize()
	{
		base.transform.localScale = new Vector3(_width + _borderOuterRadius * 2f, _height + _borderOuterRadius * 2f, 1f);
		UpdateMaterialPropertyBlock();
	}

	private void UpdateMaterialPropertyBlock()
	{
		if (_editor == null)
		{
			_editor = GetComponent<MaterialPropertyBlockEditor>();
			if (_editor == null)
			{
				return;
			}
		}
		MaterialPropertyBlock materialPropertyBlock = _editor.MaterialPropertyBlock;
		materialPropertyBlock.SetColor(_colorShaderID, _color);
		materialPropertyBlock.SetColor(_borderColorShaderID, _borderColor);
		materialPropertyBlock.SetVector(_radiiShaderID, new Vector4(_radiusTopRight, _radiusBottomRight, _radiusTopLeft, _radiusBottomLeft));
		materialPropertyBlock.SetVector(_dimensionsShaderID, new Vector4(base.transform.localScale.x, base.transform.localScale.y, _borderInnerRadius, _borderOuterRadius));
		_editor.UpdateMaterialPropertyBlock();
	}

	protected virtual void OnValidate()
	{
		UpdateSize();
		UpdateMaterialPropertyBlock();
	}
}
