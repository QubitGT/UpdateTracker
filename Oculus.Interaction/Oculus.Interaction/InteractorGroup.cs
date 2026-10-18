using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

public abstract class InteractorGroup : MonoBehaviour, IInteractor, IInteractorView, IUpdateDriver
{
	protected delegate bool InteractorPredicate(IInteractor interactor, int index);

	[SerializeField]
	[Interface(typeof(IInteractor), new Type[] { })]
	protected List<UnityEngine.Object> _interactors;

	public IReadOnlyList<IInteractor> Interactors;

	[SerializeField]
	[Interface(typeof(IActiveState), new Type[] { })]
	[Optional]
	private UnityEngine.Object _activeState;

	private IActiveState ActiveState;

	[SerializeField]
	[Interface(typeof(ICandidateComparer), new Type[] { })]
	[Optional]
	protected UnityEngine.Object _candidateComparer;

	protected ICandidateComparer CandidateComparer;

	[SerializeField]
	private int _maxIterationsPerFrame = 3;

	protected static readonly InteractorPredicate TruePredicate = (IInteractor interactor, int index) => true;

	protected static readonly InteractorPredicate HasCandidatePredicate = (IInteractor interactor, int index) => interactor.HasCandidate;

	protected static readonly InteractorPredicate HasInteractablePredicate = (IInteractor interactor, int index) => interactor.HasInteractable;

	private InteractorState _state = InteractorState.Disabled;

	private UniqueIdentifier _identifier;

	protected bool _started;

	public int MaxIterationsPerFrame
	{
		get
		{
			return _maxIterationsPerFrame;
		}
		set
		{
			_maxIterationsPerFrame = value;
		}
	}

	public object Data => null;

	public bool IsRootDriver { get; set; } = true;


	public abstract bool ShouldHover { get; }

	public abstract bool ShouldUnhover { get; }

	public abstract bool ShouldSelect { get; }

	public abstract bool ShouldUnselect { get; }

	public abstract bool HasCandidate { get; }

	public abstract bool HasInteractable { get; }

	public abstract bool HasSelectedInteractable { get; }

	public abstract object CandidateProperties { get; }

	public InteractorState State
	{
		get
		{
			return _state;
		}
		protected set
		{
			if (_state != value)
			{
				InteractorStateChangeArgs obj = new InteractorStateChangeArgs(_state, value);
				_state = value;
				this.WhenStateChanged(obj);
			}
		}
	}

	public int Identifier => _identifier.ID;

	public event Action<InteractorStateChangeArgs> WhenStateChanged = delegate
	{
	};

	public event Action WhenPreprocessed = delegate
	{
	};

	public event Action WhenProcessed = delegate
	{
	};

	public event Action WhenPostprocessed = delegate
	{
	};

	public abstract void Hover();

	public abstract void Unhover();

	public abstract void Select();

	public abstract void Unselect();

	protected virtual void Awake()
	{
		_identifier = UniqueIdentifier.Generate(Context.Global.GetInstance(), this);
		ActiveState = _activeState as IActiveState;
		if (_interactors != null)
		{
			Interactors = _interactors.FindAll((UnityEngine.Object mono) => mono != null).ConvertAll((UnityEngine.Object mono) => mono as IInteractor);
		}
		CandidateComparer = _candidateComparer as ICandidateComparer;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		for (int i = 0; i < Interactors.Count; i++)
		{
			Interactors[i].IsRootDriver = false;
		}
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Disable();
		}
	}

	protected virtual void OnDestroy()
	{
		UniqueIdentifier.Release(_identifier);
	}

	protected static int CompareStates(InteractorState a, InteractorState b)
	{
		if (a == b)
		{
			return 0;
		}
		if ((a == InteractorState.Disabled && b != InteractorState.Disabled) || (a == InteractorState.Normal && (b == InteractorState.Hover || b == InteractorState.Select)) || (a == InteractorState.Hover && b == InteractorState.Select))
		{
			return 1;
		}
		return -1;
	}

	protected bool TryGetBestCandidateIndex(InteractorPredicate predicate, out int bestCandidateIndex, int betterThan = -1, int skipIndex = -1)
	{
		bestCandidateIndex = betterThan;
		for (int i = 0; i < Interactors.Count; i++)
		{
			if (i != skipIndex)
			{
				IInteractor interactor = Interactors[i];
				if (predicate(interactor, i) && CompareCandidates(bestCandidateIndex, i) > 0)
				{
					bestCandidateIndex = i;
				}
			}
		}
		return bestCandidateIndex != betterThan;
	}

	protected bool AnyInteractor(InteractorPredicate predicate)
	{
		for (int i = 0; i < Interactors.Count; i++)
		{
			if (predicate(Interactors[i], i))
			{
				return true;
			}
		}
		return false;
	}

	protected int CompareCandidates(int indexA, int indexB)
	{
		if (indexA < 0 && indexB >= 0)
		{
			return 1;
		}
		if (indexA >= 0 && indexB < 0)
		{
			return -1;
		}
		if (indexA < 0 && indexB < 0)
		{
			return 0;
		}
		if (indexA == indexB)
		{
			return 0;
		}
		IInteractor interactor = Interactors[indexA];
		IInteractor interactor2 = Interactors[indexB];
		if (!interactor.HasCandidate && !interactor2.HasCandidate)
		{
			if (indexA >= indexB)
			{
				return 1;
			}
			return -1;
		}
		if (interactor.HasCandidate && interactor2.HasCandidate)
		{
			if (CandidateComparer == null)
			{
				if (indexA >= indexB)
				{
					return 1;
				}
				return -1;
			}
			if (CandidateComparer.Compare(interactor.CandidateProperties, interactor2.CandidateProperties) >= 0)
			{
				return 1;
			}
			return -1;
		}
		if (!interactor.HasCandidate)
		{
			return 1;
		}
		return -1;
	}

	public virtual void Preprocess()
	{
		if (!UpdateActiveState())
		{
			Disable();
		}
		else
		{
			for (int i = 0; i < Interactors.Count; i++)
			{
				Interactors[i].Preprocess();
			}
		}
		this.WhenPreprocessed();
	}

	public virtual void Process()
	{
		int num = 0;
		while (Interactors != null && num < Interactors.Count)
		{
			Interactors[num].Process();
			num++;
		}
		this.WhenProcessed();
	}

	public virtual void Postprocess()
	{
		int num = 0;
		while (Interactors != null && num < Interactors.Count)
		{
			Interactors[num].Postprocess();
			num++;
		}
		this.WhenPostprocessed();
	}

	public virtual void ProcessCandidate()
	{
		if (!UpdateActiveState())
		{
			return;
		}
		for (int i = 0; i < Interactors.Count; i++)
		{
			IInteractor interactor = Interactors[i];
			if (interactor.State == InteractorState.Hover || interactor.State == InteractorState.Normal)
			{
				interactor.ProcessCandidate();
			}
		}
	}

	public virtual void Enable()
	{
		if (UpdateActiveState())
		{
			for (int i = 0; i < Interactors.Count; i++)
			{
				Interactors[i].Enable();
			}
			if (State == InteractorState.Disabled)
			{
				State = InteractorState.Normal;
			}
		}
	}

	public virtual void Disable()
	{
		int num = 0;
		while (Interactors != null && num < Interactors.Count)
		{
			Interactors[num].Disable();
			num++;
		}
		State = InteractorState.Disabled;
	}

	protected void DisableAllExcept(IInteractor mainInteractor)
	{
		for (int i = 0; i < Interactors.Count; i++)
		{
			IInteractor interactor = Interactors[i];
			if (interactor != mainInteractor)
			{
				interactor.Disable();
			}
		}
	}

	protected void EnableAllExcept(IInteractor mainInteractor)
	{
		for (int i = 0; i < Interactors.Count; i++)
		{
			IInteractor interactor = Interactors[i];
			if (interactor != mainInteractor)
			{
				interactor.Enable();
			}
		}
	}

	protected bool UpdateActiveState()
	{
		bool flag = base.isActiveAndEnabled && _started;
		if (ActiveState != null)
		{
			flag = flag && ActiveState.Active;
		}
		return flag;
	}

	protected virtual void Update()
	{
		if (IsRootDriver)
		{
			Drive();
		}
	}

	public virtual void Drive()
	{
		Preprocess();
		if (!UpdateActiveState())
		{
			Disable();
			Postprocess();
			return;
		}
		Enable();
		for (int i = 0; i < MaxIterationsPerFrame; i++)
		{
			if (State == InteractorState.Normal || State == InteractorState.Hover)
			{
				ProcessCandidate();
			}
			Process();
			if (State == InteractorState.Disabled)
			{
				break;
			}
			if (State == InteractorState.Normal)
			{
				if (!ShouldHover)
				{
					break;
				}
				Hover();
			}
			else if (State == InteractorState.Hover)
			{
				if (ShouldSelect)
				{
					Select();
					continue;
				}
				if (!ShouldUnhover)
				{
					break;
				}
				Unhover();
			}
			else if (State == InteractorState.Select)
			{
				if (!ShouldUnselect)
				{
					break;
				}
				Unselect();
			}
		}
		Postprocess();
	}

	public void InjectAllInteractorGroupBase(List<IInteractor> interactors)
	{
		InjectInteractors(interactors);
	}

	public void InjectInteractors(List<IInteractor> interactors)
	{
		Interactors = interactors;
		_interactors = interactors.ConvertAll((IInteractor i) => i as UnityEngine.Object);
	}

	public void InjectOptionalActiveState(IActiveState activeState)
	{
		ActiveState = activeState;
		_activeState = activeState as UnityEngine.Object;
	}

	public void InjectOptionalCandidateComparer(ICandidateComparer candidateComparer)
	{
		CandidateComparer = candidateComparer;
		_candidateComparer = candidateComparer as UnityEngine.Object;
	}
}
