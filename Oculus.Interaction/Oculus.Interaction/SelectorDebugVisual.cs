using System;
using UnityEngine;

namespace Oculus.Interaction;

public class SelectorDebugVisual : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(ISelector), new Type[] { })]
	private UnityEngine.Object _selector;

	[SerializeField]
	private Renderer _renderer;

	[SerializeField]
	private Color _normalColor = Color.red;

	[SerializeField]
	private Color _selectColor = Color.green;

	private ISelector Selector;

	private Material _material;

	private bool _selected;

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
		Selector = _selector as ISelector;
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
			Selector.WhenSelected += HandleSelected;
			Selector.WhenUnselected += HandleUnselected;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			HandleUnselected();
			Selector.WhenSelected -= HandleSelected;
			Selector.WhenUnselected -= HandleUnselected;
		}
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(_material);
	}

	private void HandleSelected()
	{
		if (!_selected)
		{
			_selected = true;
			_material.color = _selectColor;
		}
	}

	private void HandleUnselected()
	{
		if (_selected)
		{
			_selected = false;
			_material.color = _normalColor;
		}
	}

	public void InjectAllSelectorDebugVisual(ISelector selector, Renderer renderer)
	{
		InjectSelector(selector);
		InjectRenderer(renderer);
	}

	public void InjectSelector(ISelector selector)
	{
		_selector = selector as UnityEngine.Object;
		Selector = selector;
	}

	public void InjectRenderer(Renderer renderer)
	{
		_renderer = renderer;
	}
}
