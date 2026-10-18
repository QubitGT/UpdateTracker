using System.Collections.Generic;

namespace Oculus.Interaction;

public class FirstHoverInteractorGroup : InteractorGroup
{
	private IInteractor _bestInteractor;

	private int _bestInteractorIndex = -1;

	private static readonly InteractorPredicate IsNormalAndShouldHoverPredicate = (IInteractor interactor, int index) => interactor.State == InteractorState.Normal && interactor.ShouldHover;

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
			if (_bestInteractor != null)
			{
				return _bestInteractor.ShouldUnhover;
			}
			return false;
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
			if (_bestInteractor != null)
			{
				return _bestInteractor.ShouldSelect;
			}
			return false;
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
			return false;
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

	private bool TryHover(int skipIndex = -1)
	{
		if (TryGetBestCandidateIndex(IsNormalAndShouldHoverPredicate, out var bestCandidateIndex, -1, skipIndex))
		{
			HoverAtIndex(bestCandidateIndex);
			return true;
		}
		return false;
	}

	private void HoverAtIndex(int interactorIndex)
	{
		UnsuscribeBestInteractor();
		_bestInteractorIndex = interactorIndex;
		_bestInteractor = Interactors[_bestInteractorIndex];
		_bestInteractor.Hover();
		_bestInteractor.WhenStateChanged += HandleBestInteractorStateChanged;
		DisableAllExcept(_bestInteractor);
	}

	public override void Unhover()
	{
		if (base.State != InteractorState.Hover)
		{
			return;
		}
		if (_bestInteractor != null)
		{
			_bestInteractor.Unhover();
			if (_bestInteractor != null && _bestInteractor.State == InteractorState.Hover)
			{
				return;
			}
			ProcessCandidate();
			TryHover(_bestInteractorIndex);
		}
		if (_bestInteractor == null)
		{
			base.State = InteractorState.Normal;
		}
	}

	public override void Select()
	{
		if (base.State == InteractorState.Hover)
		{
			_bestInteractor.Select();
			base.State = InteractorState.Select;
		}
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
		if (_bestInteractor == null && (base.State == InteractorState.Hover || base.State == InteractorState.Select))
		{
			ProcessCandidate();
			base.Process();
			if (TryHover())
			{
				if (base.State == InteractorState.Select)
				{
					_bestInteractor.Process();
					if (ShouldSelect)
					{
						Select();
						base.State = InteractorState.Select;
						return;
					}
				}
				base.State = InteractorState.Hover;
			}
			else
			{
				if (base.State == InteractorState.Select)
				{
					base.State = InteractorState.Hover;
				}
				if (base.State == InteractorState.Hover)
				{
					base.State = InteractorState.Normal;
				}
			}
		}
		else if (_bestInteractor != null && base.State == InteractorState.Select && _bestInteractor.State == InteractorState.Hover)
		{
			base.State = InteractorState.Hover;
		}
	}

	private void HandleBestInteractorStateChanged(InteractorStateChangeArgs stateChange)
	{
		if (stateChange.PreviousState == InteractorState.Hover && stateChange.NewState == InteractorState.Normal)
		{
			IInteractor bestInteractor = _bestInteractor;
			UnsuscribeBestInteractor();
			EnableAllExcept(bestInteractor);
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
			_bestInteractorIndex = -1;
		}
	}

	public void InjectAllInteractorGroupFirstHover(List<IInteractor> interactors)
	{
		InjectAllInteractorGroupBase(interactors);
	}
}
