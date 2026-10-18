using System;
using Meta.XR.Util;
using UnityEngine;

namespace Oculus.Interaction.Input.Visuals;

[Obsolete("Use ControllerVisual instead.")]
[Feature(Feature.Interaction)]
public class OVRControllerVisual : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IController), new Type[] { })]
	private UnityEngine.Object _controller;

	public IController Controller;

	[SerializeField]
	private OVRControllerHelper _ovrControllerHelper;

	protected bool _started;

	public bool ForceOffVisibility { get; set; }

	protected virtual void Awake()
	{
		Controller = _controller as IController;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		switch (Controller.Handedness)
		{
		case Handedness.Left:
			_ovrControllerHelper.m_controller = OVRInput.Controller.LTouch;
			break;
		case Handedness.Right:
			_ovrControllerHelper.m_controller = OVRInput.Controller.RTouch;
			break;
		}
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
		if (_started && _controller != null)
		{
			Controller.WhenUpdated -= HandleUpdated;
		}
	}

	private void HandleUpdated()
	{
		if (!Controller.IsConnected || ForceOffVisibility || !Controller.TryGetPose(out var pose))
		{
			_ovrControllerHelper.gameObject.SetActive(value: false);
			return;
		}
		_ovrControllerHelper.gameObject.SetActive(value: true);
		base.transform.position = pose.position;
		base.transform.rotation = pose.rotation;
		float num = ((base.transform.parent != null) ? base.transform.parent.lossyScale.x : 1f);
		base.transform.localScale = Controller.Scale / num * Vector3.one;
	}

	public void InjectAllOVRControllerVisual(IController controller, OVRControllerHelper ovrControllerHelper)
	{
		InjectController(controller);
		InjectAllOVRControllerHelper(ovrControllerHelper);
	}

	public void InjectController(IController controller)
	{
		_controller = controller as UnityEngine.Object;
		Controller = controller;
	}

	public void InjectAllOVRControllerHelper(OVRControllerHelper ovrControllerHelper)
	{
		_ovrControllerHelper = ovrControllerHelper;
	}
}
