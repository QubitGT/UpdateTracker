using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Throw;

public class ControllerPoseInputDevice : MonoBehaviour, IPoseInputDevice
{
	[SerializeField]
	[Interface(typeof(IController), new Type[] { })]
	private UnityEngine.Object _controller;

	public IController Controller { get; private set; }

	public bool IsInputValid
	{
		get
		{
			if (Controller.IsConnected)
			{
				return Controller.IsPoseValid;
			}
			return false;
		}
	}

	public bool IsHighConfidence => IsInputValid;

	public bool GetRootPose(out Pose pose)
	{
		pose = Pose.identity;
		if (!IsInputValid)
		{
			return false;
		}
		if (!Controller.TryGetPose(out pose))
		{
			return false;
		}
		return true;
	}

	protected virtual void Awake()
	{
		Controller = _controller as IController;
	}

	protected virtual void Start()
	{
	}

	public (Vector3, Vector3) GetExternalVelocities()
	{
		return (Vector3.zero, Vector3.zero);
	}

	public void InjectAllControllerPoseInputDevice(IController controller)
	{
		InjectController(controller);
	}

	public void InjectController(IController controller)
	{
		_controller = controller as UnityEngine.Object;
		Controller = controller;
	}
}
