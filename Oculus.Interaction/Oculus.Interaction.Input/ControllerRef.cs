using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public class ControllerRef : MonoBehaviour, IController, IActiveState
{
	[SerializeField]
	[Interface(typeof(IController), new Type[] { })]
	private UnityEngine.Object _controller;

	private IController Controller;

	public Handedness Handedness => Controller.Handedness;

	public bool IsConnected => Controller.IsConnected;

	public bool IsPoseValid => Controller.IsPoseValid;

	public ControllerInput ControllerInput => Controller.ControllerInput;

	public bool Active => IsConnected;

	public float Scale => Controller.Scale;

	public event Action WhenUpdated
	{
		add
		{
			Controller.WhenUpdated += value;
		}
		remove
		{
			Controller.WhenUpdated -= value;
		}
	}

	protected virtual void Awake()
	{
		Controller = _controller as IController;
	}

	protected virtual void Start()
	{
	}

	public bool TryGetPose(out Pose pose)
	{
		return Controller.TryGetPose(out pose);
	}

	public bool TryGetPointerPose(out Pose pose)
	{
		return Controller.TryGetPointerPose(out pose);
	}

	public bool IsButtonUsageAnyActive(ControllerButtonUsage buttonUsage)
	{
		return Controller.IsButtonUsageAnyActive(buttonUsage);
	}

	public bool IsButtonUsageAllActive(ControllerButtonUsage buttonUsage)
	{
		return Controller.IsButtonUsageAllActive(buttonUsage);
	}

	public void InjectAllControllerRef(IController controller)
	{
		InjectController(controller);
	}

	public void InjectController(IController controller)
	{
		_controller = controller as UnityEngine.Object;
		Controller = controller;
	}
}
