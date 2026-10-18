using System;
using UnityEngine;

namespace Oculus.Interaction;

public class InteractableDebugVisual : MonoBehaviour
{
	[Tooltip("The interactable to monitor for state changes.")]
	[SerializeField]
	[Interface(typeof(IInteractableView), new Type[] { })]
	private UnityEngine.Object _interactableView;

	[Tooltip("The mesh that will change color based on the current state.")]
	[SerializeField]
	private Renderer _renderer;

	[Tooltip("Displayed when the state is normal.")]
	[SerializeField]
	private Color _normalColor = Color.red;

	[Tooltip("Displayed when the state is hover.")]
	[SerializeField]
	private Color _hoverColor = Color.blue;

	[Tooltip("Displayed when the state is selected.")]
	[SerializeField]
	private Color _selectColor = Color.green;

	[Tooltip("Displayed when the state is disabled.")]
	[SerializeField]
	private Color _disabledColor = Color.black;

	private IInteractableView InteractableView;

	private Material _material;

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

	public Color DisabledColor
	{
		get
		{
			return _disabledColor;
		}
		set
		{
			_disabledColor = value;
		}
	}

	protected virtual void Awake()
	{
		InteractableView = _interactableView as IInteractableView;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_material = _renderer.material;
		UpdateVisual();
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			InteractableView.WhenStateChanged += UpdateVisualState;
			UpdateVisual();
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			InteractableView.WhenStateChanged -= UpdateVisualState;
		}
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(_material);
	}

	public void SetNormalColor(Color color)
	{
		_normalColor = color;
		UpdateVisual();
	}

	private void UpdateVisual()
	{
		switch (InteractableView.State)
		{
		case InteractableState.Normal:
			_material.color = _normalColor;
			break;
		case InteractableState.Hover:
			_material.color = _hoverColor;
			break;
		case InteractableState.Select:
			_material.color = _selectColor;
			break;
		case InteractableState.Disabled:
			_material.color = _disabledColor;
			break;
		}
	}

	private void UpdateVisualState(InteractableStateChangeArgs args)
	{
		UpdateVisual();
	}

	public void InjectAllInteractableDebugVisual(IInteractableView interactableView, Renderer renderer)
	{
		InjectInteractableView(interactableView);
		InjectRenderer(renderer);
	}

	public void InjectInteractableView(IInteractableView interactableView)
	{
		_interactableView = interactableView as UnityEngine.Object;
		InteractableView = interactableView;
	}

	public void InjectRenderer(Renderer renderer)
	{
		_renderer = renderer;
	}
}
