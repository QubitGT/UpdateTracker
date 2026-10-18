using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class ControllerAxis2D : MonoBehaviour, IAxis2D
{
	[SerializeField]
	[Interface(typeof(IController), new Type[] { })]
	private UnityEngine.Object _controller;

	[SerializeField]
	private ControllerAxis2DUsage _axis = ControllerAxis2DUsage.Primary2DAxis;

	protected bool _started;

	private IController Controller { get; set; }

	public ControllerAxis2DUsage Axis
	{
		get
		{
			return _axis;
		}
		set
		{
			_axis = value;
		}
	}

	protected virtual void Awake()
	{
		Controller = _controller as IController;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	public Vector2 Value()
	{
		Vector2 result = Vector2.zero;
		if (!_started)
		{
			return result;
		}
		if ((_axis & ControllerAxis2DUsage.Primary2DAxis) != 0)
		{
			result = Controller.ControllerInput.Primary2DAxis;
		}
		if ((_axis & ControllerAxis2DUsage.Secondary2DAxis) != 0)
		{
			result += Controller.ControllerInput.Secondary2DAxis;
		}
		return result;
	}

	public void InjectAllControllerAxis2DActiveState(IController controller)
	{
		InjectController(controller);
	}

	public void InjectController(IController controller)
	{
		Controller = controller;
		_controller = controller as UnityEngine.Object;
	}
}
