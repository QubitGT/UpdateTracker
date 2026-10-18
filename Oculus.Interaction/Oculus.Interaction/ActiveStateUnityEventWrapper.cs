using System;
using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction;

public class ActiveStateUnityEventWrapper : MonoBehaviour
{
	[Tooltip("Events will fire based on the state of this IActiveState.")]
	[SerializeField]
	[Interface(typeof(IActiveState), new Type[] { })]
	private UnityEngine.Object _activeState;

	private IActiveState ActiveState;

	[Tooltip("This event will be fired when the provided IActiveState becomes active.")]
	[SerializeField]
	private UnityEvent _whenActivated;

	[Tooltip("This event will be fired when the provided IActiveState becomes inactive.")]
	[SerializeField]
	private UnityEvent _whenDeactivated;

	[SerializeField]
	[Tooltip("If true, the corresponding event will be fired at the beginning of Update.")]
	private bool _emitOnFirstUpdate = true;

	private bool _emittedOnFirstUpdate;

	private bool _savedState;

	public UnityEvent WhenActivated => _whenActivated;

	public UnityEvent WhenDeactivated => _whenDeactivated;

	protected virtual void Awake()
	{
		ActiveState = _activeState as IActiveState;
	}

	protected virtual void Start()
	{
		_savedState = false;
	}

	protected virtual void Update()
	{
		if (_emitOnFirstUpdate && !_emittedOnFirstUpdate)
		{
			InvokeEvent();
			_emittedOnFirstUpdate = true;
		}
		bool active = ActiveState.Active;
		if (_savedState != active)
		{
			_savedState = active;
			InvokeEvent();
		}
	}

	private void InvokeEvent()
	{
		if (_savedState)
		{
			_whenActivated.Invoke();
		}
		else
		{
			_whenDeactivated.Invoke();
		}
	}

	public void InjectAllActiveStateUnityEventWrapper(IActiveState activeState)
	{
		InjectActiveState(activeState);
	}

	public void InjectActiveState(IActiveState activeState)
	{
		_activeState = activeState as UnityEngine.Object;
		ActiveState = activeState;
	}

	public void InjectOptionalEmitOnFirstUpdate(bool emitOnFirstUpdate)
	{
		_emitOnFirstUpdate = emitOnFirstUpdate;
	}

	public void InjectOptionalWhenActivated(UnityEvent whenActivated)
	{
		_whenActivated = whenActivated;
	}

	public void InjectOptionalWhenDeactivated(UnityEvent whenDeactivated)
	{
		_whenDeactivated = whenDeactivated;
	}
}
