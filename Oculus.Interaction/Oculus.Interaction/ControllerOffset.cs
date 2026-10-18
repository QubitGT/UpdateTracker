using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class ControllerOffset : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IController), new Type[] { })]
	private UnityEngine.Object _controller;

	[SerializeField]
	private Vector3 _offset;

	[SerializeField]
	private Quaternion _rotation = Quaternion.identity;

	protected bool _started;

	public IController Controller { get; private set; }

	protected virtual void Awake()
	{
		Controller = _controller as IController;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			Controller.WhenUpdated += HandleUpdated;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Controller.WhenUpdated -= HandleUpdated;
		}
	}

	private void HandleUpdated()
	{
		if (Controller.TryGetPose(out var pose))
		{
			Pose a = new Pose(Controller.Scale * _offset, _rotation);
			a.Postmultiply(in pose);
			base.transform.SetPose(in a);
		}
	}

	public void GetOffset(ref Pose pose)
	{
		pose.position = Controller.Scale * _offset;
		pose.rotation = _rotation;
	}

	public void GetWorldPose(ref Pose pose)
	{
		pose.position = base.transform.position;
		pose.rotation = base.transform.rotation;
	}

	public void InjectController(IController controller)
	{
		_controller = controller as UnityEngine.Object;
		Controller = controller;
	}

	public void InjectOffset(Vector3 offset)
	{
		_offset = offset;
	}

	public void InjectRotation(Quaternion rotation)
	{
		_rotation = rotation;
	}

	public void InjectAllControllerOffset(IController controller, Vector3 offset, Quaternion rotation)
	{
		InjectController(controller);
		InjectOffset(offset);
		InjectRotation(rotation);
	}
}
