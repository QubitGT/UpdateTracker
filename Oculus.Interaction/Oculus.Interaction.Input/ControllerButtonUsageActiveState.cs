using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public class ControllerButtonUsageActiveState : MonoBehaviour, IActiveState
{
	[SerializeField]
	[Interface(typeof(IController), new Type[] { })]
	private UnityEngine.Object _controller;

	private IController Controller;

	[SerializeField]
	private ControllerButtonUsage _controllerButtonUsage;

	public bool Active => Controller.IsButtonUsageAnyActive(_controllerButtonUsage);

	protected virtual void Awake()
	{
		Controller = _controller as IController;
	}

	protected virtual void Start()
	{
	}

	public void InjectAllControllerButtonUsageActiveState(IController controller, ControllerButtonUsage controllerButtonUsage)
	{
		InjectController(controller);
		InjectControllerButtonUsage(controllerButtonUsage);
	}

	public void InjectController(IController controller)
	{
		_controller = controller as UnityEngine.Object;
		Controller = controller;
	}

	public void InjectControllerButtonUsage(ControllerButtonUsage controllerButtonUsage)
	{
		_controllerButtonUsage = controllerButtonUsage;
	}
}
