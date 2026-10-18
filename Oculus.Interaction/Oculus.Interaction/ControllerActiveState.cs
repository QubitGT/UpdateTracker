using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class ControllerActiveState : MonoBehaviour, IActiveState
{
	[Tooltip("ActiveState will be true while this controller is connected.")]
	[SerializeField]
	[Interface(typeof(IController), new Type[] { })]
	private UnityEngine.Object _controller;

	private IController Controller;

	public bool Active => Controller.IsConnected;

	protected virtual void Awake()
	{
		Controller = _controller as IController;
	}

	protected virtual void Start()
	{
	}

	public void InjectAllControllerActiveState(IController controller)
	{
		InjectController(controller);
	}

	public void InjectController(IController controller)
	{
		_controller = controller as UnityEngine.Object;
		Controller = controller;
	}
}
