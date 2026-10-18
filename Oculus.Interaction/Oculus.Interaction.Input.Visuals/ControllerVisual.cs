using System;
using UnityEngine;

namespace Oculus.Interaction.Input.Visuals;

public class ControllerVisual : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IController), new Type[] { })]
	private UnityEngine.Object _controller;

	[SerializeField]
	private GameObject _root;

	private bool _started;

	public IController Controller { get; private set; }

	public bool ForceOffVisibility { get; set; }

	protected virtual void Awake()
	{
		if (Controller == null)
		{
			Controller = _controller as IController;
		}
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
		if (_started && _controller != null)
		{
			Controller.WhenUpdated -= HandleUpdated;
		}
	}

	private void HandleUpdated()
	{
		if (!Controller.IsConnected || ForceOffVisibility || !Controller.TryGetPose(out var pose))
		{
			_root.SetActive(value: false);
			return;
		}
		_root.SetActive(value: true);
		_root.transform.position = pose.position;
		_root.transform.rotation = pose.rotation;
		float num = ((_root.transform.parent != null) ? _root.transform.parent.lossyScale.x : 1f);
		_root.transform.localScale = Controller.Scale / num * Vector3.one;
	}

	public void InjectAllOVRControllerVisual(IController controller, GameObject root)
	{
		InjectController(controller);
		InjectRoot(root);
	}

	public void InjectController(IController controller)
	{
		_controller = controller as UnityEngine.Object;
		Controller = controller;
	}

	public void InjectRoot(GameObject root)
	{
		_root = root;
	}
}
