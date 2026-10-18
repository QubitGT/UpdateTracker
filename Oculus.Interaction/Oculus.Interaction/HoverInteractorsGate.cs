using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

public class HoverInteractorsGate : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IInteractor), new Type[] { })]
	private List<UnityEngine.Object> _interactorsA;

	private List<IInteractor> InteractorsA;

	[SerializeField]
	[Interface(typeof(IInteractor), new Type[] { })]
	private List<UnityEngine.Object> _interactorsB;

	private List<IInteractor> InteractorsB;

	private int _hoveringInteractorsACount;

	private int _hoveringInteractorsBCount;

	protected bool _started;

	protected virtual void Awake()
	{
		InteractorsA = _interactorsA.FindAll((UnityEngine.Object i) => i != null).ConvertAll((UnityEngine.Object i) => i as IInteractor);
		InteractorsB = _interactorsB.FindAll((UnityEngine.Object i) => i != null).ConvertAll((UnityEngine.Object i) => i as IInteractor);
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (!_started)
		{
			return;
		}
		foreach (IInteractor item in InteractorsA)
		{
			item.WhenStateChanged += HandleInteractorAStateChanged;
		}
		foreach (IInteractor item2 in InteractorsB)
		{
			item2.WhenStateChanged += HandleInteractorBStateChanged;
		}
	}

	protected virtual void OnDisable()
	{
		if (!_started)
		{
			return;
		}
		foreach (IInteractor item in InteractorsA)
		{
			item.WhenStateChanged -= HandleInteractorAStateChanged;
		}
		foreach (IInteractor item2 in InteractorsB)
		{
			item2.WhenStateChanged -= HandleInteractorBStateChanged;
		}
		_hoveringInteractorsACount = 0;
		_hoveringInteractorsBCount = 0;
	}

	private void HandleInteractorAStateChanged(InteractorStateChangeArgs stateChange)
	{
		ProcessInteractorsStateChange(stateChange, ref _hoveringInteractorsACount, InteractorsB);
	}

	private void HandleInteractorBStateChanged(InteractorStateChangeArgs stateChange)
	{
		ProcessInteractorsStateChange(stateChange, ref _hoveringInteractorsBCount, InteractorsA);
	}

	private void ProcessInteractorsStateChange(InteractorStateChangeArgs stateChange, ref int hoveringCounter, List<IInteractor> oppositeInteractors)
	{
		if (stateChange.PreviousState == InteractorState.Normal && stateChange.NewState == InteractorState.Hover && hoveringCounter++ == 0)
		{
			EnableAll(oppositeInteractors, enable: false);
		}
		if (stateChange.PreviousState == InteractorState.Hover && stateChange.NewState == InteractorState.Normal && --hoveringCounter == 0)
		{
			EnableAll(oppositeInteractors, enable: true);
		}
	}

	private void EnableAll(List<IInteractor> interactors, bool enable)
	{
		foreach (IInteractor interactor in interactors)
		{
			if (interactor is Behaviour behaviour)
			{
				behaviour.enabled = enable;
			}
		}
	}

	public void InjectAllHoverInteractorsGate(List<IInteractor> interactorsA, List<IInteractor> interactorsB)
	{
		InjectInteractorsA(interactorsA);
		InjectInteractorsB(interactorsB);
	}

	public void InjectInteractorsA(List<IInteractor> interactors)
	{
		InteractorsA = interactors;
		_interactorsA = interactors.ConvertAll((IInteractor i) => i as UnityEngine.Object);
	}

	public void InjectInteractorsB(List<IInteractor> interactors)
	{
		InteractorsB = interactors;
		_interactorsB = interactors.ConvertAll((IInteractor i) => i as UnityEngine.Object);
	}
}
