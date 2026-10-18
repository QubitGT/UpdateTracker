using System;
using UnityEngine;

namespace Oculus.Interaction;

public class PointableDebugVisual : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IPointable), new Type[] { })]
	private UnityEngine.Object _pointable;

	[SerializeField]
	private Renderer _renderer;

	[SerializeField]
	private Color _normalColor = Color.red;

	[SerializeField]
	private Color _hoverColor = Color.blue;

	[SerializeField]
	private Color _selectColor = Color.green;

	private IPointable Pointable;

	private Material _material;

	private bool _hover;

	private bool _select;

	protected bool _started;

	public Color NormalColor
	{
		get
		{
			return _normalColor;
		}
		set
		{
			_normalColor = value;
		}
	}

	public Color HoverColor
	{
		get
		{
			return _hoverColor;
		}
		set
		{
			_hoverColor = value;
		}
	}

	public Color SelectColor
	{
		get
		{
			return _selectColor;
		}
		set
		{
			_selectColor = value;
		}
	}

	protected virtual void Awake()
	{
		Pointable = _pointable as IPointable;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_material = _renderer.material;
		_material.color = _normalColor;
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			Pointable.WhenPointerEventRaised += HandlePointerEventRaised;
			UpdateMaterialColor();
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Pointable.WhenPointerEventRaised -= HandlePointerEventRaised;
		}
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(_material);
	}

	private void HandlePointerEventRaised(PointerEvent evt)
	{
		switch (evt.Type)
		{
		case PointerEventType.Hover:
			_hover = true;
			UpdateMaterialColor();
			break;
		case PointerEventType.Select:
			_select = true;
			UpdateMaterialColor();
			break;
		case PointerEventType.Unselect:
			_select = false;
			UpdateMaterialColor();
			break;
		case PointerEventType.Unhover:
			_hover = false;
			UpdateMaterialColor();
			break;
		case PointerEventType.Move:
			break;
		}
	}

	private void UpdateMaterialColor()
	{
		_material.color = (_select ? _selectColor : (_hover ? _hoverColor : _normalColor));
	}

	public void InjectAllPointableDebugVisual(IPointable pointable, Renderer renderer)
	{
		InjectPointable(pointable);
		InjectRenderer(renderer);
	}

	public void InjectPointable(IPointable pointable)
	{
		_pointable = pointable as UnityEngine.Object;
		Pointable = pointable;
	}

	public void InjectRenderer(Renderer renderer)
	{
		_renderer = renderer;
	}
}
