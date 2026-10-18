using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class ControllerSelector : MonoBehaviour, ISelector
{
	public enum ControllerSelectorLogicOperator
	{
		Any,
		All
	}

	[Tooltip("The controller to check.")]
	[SerializeField]
	[Interface(typeof(IController), new Type[] { })]
	private UnityEngine.Object _controller;

	[Tooltip("The buttons to check.")]
	[SerializeField]
	private ControllerButtonUsage _controllerButtonUsage;

	[Tooltip("Determines how many of the checked buttons must be pressed for the controller to be selecting. 'All' requires all of the buttons to be pressed. 'Any' requires only one to be pressed.")]
	[SerializeField]
	private ControllerSelectorLogicOperator _requireButtonUsages;

	private bool _selected;

	public ControllerButtonUsage ControllerButtonUsage
	{
		get
		{
			return _controllerButtonUsage;
		}
		set
		{
			_controllerButtonUsage = value;
		}
	}

	public ControllerSelectorLogicOperator RequireButtonUsages
	{
		get
		{
			return _requireButtonUsages;
		}
		set
		{
			_requireButtonUsages = value;
		}
	}

	public IController Controller { get; private set; }

	public event Action WhenSelected = delegate
	{
	};

	public event Action WhenUnselected = delegate
	{
	};

	protected virtual void Awake()
	{
		Controller = _controller as IController;
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
		if ((_requireButtonUsages == ControllerSelectorLogicOperator.All) ? Controller.IsButtonUsageAllActive(_controllerButtonUsage) : Controller.IsButtonUsageAnyActive(_controllerButtonUsage))
		{
			if (!_selected)
			{
				_selected = true;
				this.WhenSelected();
			}
		}
		else if (_selected)
		{
			_selected = false;
			this.WhenUnselected();
		}
	}

	public void InjectAllControllerSelector(IController controller)
	{
		InjectController(controller);
	}

	public void InjectController(IController controller)
	{
		_controller = controller as UnityEngine.Object;
		Controller = controller;
	}
}
