using System;
using UnityEngine;

namespace Oculus.Interaction;

public class InteractorActiveState : MonoBehaviour, IActiveState
{
	[Flags]
	public enum InteractorProperty
	{
		HasCandidate = 1,
		HasInteractable = 2,
		IsSelecting = 4,
		HasSelectedInteractable = 8,
		IsNormal = 0x10,
		IsHovering = 0x20,
		IsDisabled = 0x40
	}

	[SerializeField]
	[Interface(typeof(IInteractor), new Type[] { })]
	private UnityEngine.Object _interactor;

	private IInteractor Interactor;

	[SerializeField]
	private InteractorProperty _property;

	public InteractorProperty Property
	{
		get
		{
			return _property;
		}
		set
		{
			_property = value;
		}
	}

	public bool Active
	{
		get
		{
			if (!base.isActiveAndEnabled)
			{
				return false;
			}
			if ((_property & InteractorProperty.HasCandidate) != 0 && Interactor.HasCandidate)
			{
				return true;
			}
			if ((_property & InteractorProperty.HasInteractable) != 0 && Interactor.HasInteractable)
			{
				return true;
			}
			if ((_property & InteractorProperty.IsSelecting) != 0 && Interactor.State == InteractorState.Select)
			{
				return true;
			}
			if ((_property & InteractorProperty.HasSelectedInteractable) != 0 && Interactor.HasSelectedInteractable)
			{
				return true;
			}
			if ((_property & InteractorProperty.IsNormal) != 0 && Interactor.State == InteractorState.Normal)
			{
				return true;
			}
			if ((_property & InteractorProperty.IsHovering) != 0 && Interactor.State == InteractorState.Hover)
			{
				return true;
			}
			if ((_property & InteractorProperty.IsDisabled) != 0 && Interactor.State == InteractorState.Disabled)
			{
				return true;
			}
			return false;
		}
	}

	protected virtual void Awake()
	{
		Interactor = _interactor as IInteractor;
	}

	protected virtual void Start()
	{
	}

	public void InjectAllInteractorActiveState(IInteractor interactor)
	{
		InjectInteractor(interactor);
	}

	public void InjectInteractor(IInteractor interactor)
	{
		_interactor = interactor as UnityEngine.Object;
		Interactor = interactor;
	}
}
