using System;
using UnityEngine;

namespace Oculus.Interaction;

public class ActiveStateGate : MonoBehaviour, IActiveState
{
	[SerializeField]
	[Interface(typeof(ISelector), new Type[] { })]
	private UnityEngine.Object _openSelector;

	[SerializeField]
	[Interface(typeof(ISelector), new Type[] { })]
	private UnityEngine.Object _closeSelector;

	protected bool _started;

	private ISelector OpenSelector { get; set; }

	private ISelector CloseSelector { get; set; }

	public bool Active { get; private set; }

	protected virtual void Awake()
	{
		OpenSelector = _openSelector as ISelector;
		CloseSelector = _closeSelector as ISelector;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	private void OnEnable()
	{
		if (_started)
		{
			OpenSelector.WhenSelected += HandleOpenSelected;
			CloseSelector.WhenSelected += HandleCloseSelected;
		}
	}

	private void OnDisable()
	{
		if (_started)
		{
			Active = false;
			OpenSelector.WhenSelected -= HandleOpenSelected;
			CloseSelector.WhenSelected -= HandleCloseSelected;
		}
	}

	private void HandleOpenSelected()
	{
		Active = true;
	}

	private void HandleCloseSelected()
	{
		Active = false;
	}

	public void InjectAllActiveStateGate(ISelector openSelector, ISelector closeSelector)
	{
		InjectOpenState(openSelector);
		InjectCloseState(closeSelector);
	}

	public void InjectOpenState(ISelector openSelector)
	{
		_openSelector = openSelector as UnityEngine.Object;
		OpenSelector = openSelector;
	}

	public void InjectCloseState(ISelector closeSelector)
	{
		_closeSelector = closeSelector as UnityEngine.Object;
		CloseSelector = closeSelector;
	}
}
