using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class ControllerPointerPose : MonoBehaviour, IActiveState
{
	[Tooltip("A controller ray interactor.")]
	[SerializeField]
	[Interface(typeof(IController), new Type[] { })]
	private UnityEngine.Object _controller;

	[Tooltip("How much the ray origin is offset relative to the controller.")]
	[SerializeField]
	private Vector3 _offset;

	protected bool _started;

	public IController Controller { get; private set; }

	public bool Active { get; private set; }

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
		if (Controller.TryGetPointerPose(out var pose))
		{
			pose.position += pose.rotation * (Controller.Scale * _offset);
			base.transform.SetPose(in pose);
			Active = true;
		}
		else
		{
			Active = false;
		}
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

	public void InjectAllControllerPointerPose(IController controller, Vector3 offset)
	{
		InjectController(controller);
		InjectOffset(offset);
	}
}
