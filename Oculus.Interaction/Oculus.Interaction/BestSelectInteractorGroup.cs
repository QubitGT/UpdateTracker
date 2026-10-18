using System;
using System.Collections.Generic;

namespace Oculus.Interaction;

public class BestSelectInteractorGroup : InteractorGroup
{
	private IInteractor _bestInteractor;

	private static readonly InteractorPredicate IsNormalAndShouldHoverPredicate = (IInteractor interactor, int index) => interactor.State == InteractorState.Normal && interactor.ShouldHover;

	private static readonly InteractorPredicate IsHoverAndShouldUnhoverPredicate = (IInteractor interactor, int index) => interactor.State == InteractorState.Hover && interactor.ShouldUnhover;

	private static readonly InteractorPredicate IsHoverAndShouldSelectPredicate = (IInteractor interactor, int index) => interactor.State == InteractorState.Hover && interactor.ShouldSelect;

	private static readonly InteractorPredicate IsHover = (IInteractor interactor, int index) => interactor.State == InteractorState.Hover;

	public override bool ShouldHover
	{
		get
		{
			if (base.State != 0)
			{
				return false;
			}
			return AnyInteractor(IsNormalAndShouldHoverPredicate);
		}
	}

	public override bool ShouldUnhover
	{
		get
		{
			if (base.State != InteractorState.Hover)
			{
				return false;
			}
			if (!AnyInteractor(IsHoverAndShouldUnhoverPredicate))
			{
				return !AnyInteractor(IsHover);
			}
			return true;
		}
	}

	public override bool ShouldSelect
	{
		get
		{
			if (base.State != InteractorState.Hover)
			{
				return false;
			}
			return AnyInteractor(IsHoverAndShouldSelectPredicate);
		}
	}

	public override bool ShouldUnselect
	{
		get
		{
			if (base.State != InteractorState.Select)
			{
				return false;
			}
			if (_bestInteractor != null)
			{
				return _bestInteractor.ShouldUnselect;
			}
			return false;
		}
	}

	public override bool HasCandidate
	{
		get
		{
			if (_bestInteractor != null && _bestInteractor.HasCandidate)
			{
				return true;
			}
			return AnyInteractor(InteractorGroup.HasCandidatePredicate);
		}
	}

	public override bool HasInteractable
	{
		get
		{
			if (_bestInteractor != null)
			{
				return _bestInteractor.HasInteractable;
			}
			return AnyInteractor(InteractorGroup.HasInteractablePredicate);
		}
	}

	public override bool HasSelectedInteractable
	{
		get
		{
			if (_bestInteractor != null)
			{
				return _bestInteractor.HasSelectedInteractable;
			}
			return false;
		}
	}

	public override object CandidateProperties
	{
		get
		{
			if (_bestInteractor != null && _bestInteractor.HasCandidate)
			{
				return _bestInteractor.CandidateProperties;
			}
			if (TryGetBestCandidateIndex(InteractorGroup.TruePredicate, out var bestCandidateIndex))
			{
				return Interactors[bestCandidateIndex].CandidateProperties;
			}
			return null;
		}
	}

	public override void Hover()
	{
		if (TryHover())
		{
			base.State = InteractorState.Hover;
		}
	}

	private bool TryHover(Action<IInteractor> whenHover = null)
	{
		bool result = false;
		int bestCandidateIndex;
		while (TryGetBestCandidateIndex(IsNormalAndShouldHoverPredicate, out bestCandidateIndex))
		{
			Interactors[bestCandidateIndex].Hover();
			whenHover?.Invoke(Interactors[bestCandidateIndex]);
			result = true;
		}
		return result;
	}

	public override void Unhover()
	{
		if (base.State == InteractorState.Hover)
		{
			int bestCandidateIndex;
			while (TryGetBestCandidateIndex(IsHoverAndShouldUnhoverPredicate, out bestCandidateIndex))
			{
				Interactors[bestCandidateIndex].Unhover();
			}
			if (!AnyInteractor(IsHover))
			{
				base.State = InteractorState.Normal;
			}
		}
	}

	public override void Select()
	{
		if (TryGetBestCandidateIndex(IsHoverAndShouldSelectPredicate, out var bestCandidateIndex))
		{
			_bestInteractor = Interactors[bestCandidateIndex];
			_bestInteractor.Select();
			_bestInteractor.WhenStateChanged += HandleBestInteractorStateChanged;
			DisableAllExcept(_bestInteractor);
		}
		base.State = InteractorState.Select;
	}

	public override void Unselect()
	{
		if (base.State != InteractorState.Select)
		{
			return;
		}
		if (_bestInteractor != null)
		{
			_bestInteractor.Unselect();
			if (_bestInteractor != null && _bestInteractor.State == InteractorState.Select)
			{
				return;
			}
		}
		base.State = InteractorState.Hover;
	}

	public override void Preprocess()
	{
		base.Preprocess();
		if (_bestInteractor == null && base.State == InteractorState.Select)
		{
			ProcessCandidate();
			base.Process();
			if (TryHover(delegate(IInteractor interactor)
			{
				interactor.Process();
			}))
			{
				if (ShouldSelect)
				{
					Select();
					base.State = InteractorState.Select;
				}
				else
				{
					base.State = InteractorState.Hover;
				}
				return;
			}
			if (base.State == InteractorState.Select)
			{
				base.State = InteractorState.Hover;
			}
			if (base.State == InteractorState.Hover)
			{
				base.State = InteractorState.Normal;
			}
		}
		else if (_bestInteractor != null && base.State == InteractorState.Select && _bestInteractor.State == InteractorState.Hover)
		{
			base.State = InteractorState.Hover;
		}
	}

	public override void Process()
	{
		base.Process();
		if (base.State == InteractorState.Hover && AnyInteractor(IsNormalAndShouldHoverPredicate) && TryHover(delegate(IInteractor interactor)
		{
			interactor.Process();
		}))
		{
			base.State = InteractorState.Hover;
		}
		if (base.State != InteractorState.Hover || !AnyInteractor(IsHoverAndShouldUnhoverPredicate))
		{
			return;
		}
		int bestCandidateIndex;
		while (TryGetBestCandidateIndex(IsHoverAndShouldUnhoverPredicate, out bestCandidateIndex))
		{
			IInteractor interactor2 = Interactors[bestCandidateIndex];
			interactor2.Unhover();
			if (interactor2.State != InteractorState.Hover)
			{
				interactor2.Process();
			}
		}
	}

	public override void Enable()
	{
		if (_bestInteractor != null)
		{
			_bestInteractor.Enable();
		}
		else
		{
			base.Enable();
		}
	}

	public override void Disable()
	{
		UnsuscribeBestInteractor();
		base.Disable();
	}

	private void UnsuscribeBestInteractor()
	{
		if (_bestInteractor != null)
		{
			_bestInteractor.WhenStateChanged -= HandleBestInteractorStateChanged;
			_bestInteractor = null;
		}
	}

	private void HandleBestInteractorStateChanged(InteractorStateChangeArgs stateChange)
	{
		if (stateChange.PreviousState == InteractorState.Select && stateChange.NewState == InteractorState.Hover)
		{
			IInteractor bestInteractor = _bestInteractor;
			UnsuscribeBestInteractor();
			EnableAllExcept(bestInteractor);
		}
	}

	public void InjectAllInteractorGroupBestSelect(List<IInteractor> interactors)
	{
		InjectAllInteractorGroupBase(interactors);
	}
}
