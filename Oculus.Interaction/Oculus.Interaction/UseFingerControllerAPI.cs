using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class UseFingerControllerAPI : MonoBehaviour, IFingerUseAPI
{
	[SerializeField]
	[Interface(typeof(IController), new Type[] { })]
	private UnityEngine.Object _controller;

	protected bool _started;

	private IController Controller { get; set; }

	protected virtual void Awake()
	{
		Controller = _controller as IController;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	public float GetFingerUseStrength(HandFinger finger)
	{
		return finger switch
		{
			HandFinger.Index => Controller.ControllerInput.Trigger, 
			HandFinger.Middle => Controller.ControllerInput.Grip, 
			HandFinger.Thumb => Mathf.Max(Controller.ControllerInput.Trigger, Controller.ControllerInput.Grip), 
			_ => 0f, 
		};
	}

	public void InjectAllUseFingerRawPinchAPI(IController controller)
	{
		InjectController(controller);
	}

	public void InjectController(IController controller)
	{
		_controller = controller as UnityEngine.Object;
		Controller = controller;
	}
}
