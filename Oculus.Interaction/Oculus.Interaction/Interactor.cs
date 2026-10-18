using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

public abstract class Interactor<TInteractor, TInteractable> : MonoBehaviour, IInteractor, IInteractorView, IUpdateDriver where TInteractor : Interactor<TInteractor, TInteractable> where TInteractable : Interactable<TInteractor, TInteractable>
{
	private const ulong DefaultNativeId = 5282254251404903456uL;

	protected ulong _nativeId = 5282254251404903456uL;

	[Tooltip("An ActiveState whose value determines if the interactor is enabled or disabled.")]
	[SerializeField]
	[Interface(typeof(IActiveState), new Type[] { })]
	[Optional]
	private UnityEngine.Object _activeState;

	private IActiveState ActiveState;

	[Tooltip("The interactables this interactor can or can't use. Is determined by comparing this interactor's TagSetFilter component(s) to the TagSet component on the interactables.")]
	[SerializeField]
	[Interface(typeof(IGameObjectFilter), new Type[] { })]
	[Optional]
	private List<UnityEngine.Object> _interactableFilters = new List<UnityEngine.Object>();

	private List<IGameObjectFilter> InteractableFilters;

	[Tooltip("Custom logic used to determine the best interactable candidate.")]
	[SerializeField]
	[Interface("CandidateTiebreaker")]
	[Optional]
	private UnityEngine.Object _candidateTiebreaker;

	private IComparer<TInteractable> CandidateTiebreaker;

	private Func<TInteractable> _computeCandidateOverride;

	private bool _clearComputeCandidateOverrideOnSelect;

	private Func<bool> _computeShouldSelectOverride;

	private bool _clearComputeShouldSelectOverrideOnSelect;

	private Func<bool> _computeShouldUnselectOverride;

	private bool _clearComputeShouldUnselectOverrideOnUnselect;

	private InteractorState _state = InteractorState.Disabled;

	private ISelector _selector;

	[Tooltip("The maximum number of state changes that can occur per frame. For example, the interactor switching from normal to hover or vice-versa counts as one state change.")]
	[SerializeField]
	private int _maxIterationsPerFrame = 3;

	private Queue<bool> _selectorQueue = new Queue<bool>();

	protected TInteractable _candidate;

	protected TInteractable _interactable;

	protected TInteractable _selectedInteractable;

	private MultiAction<TInteractable> _whenInteractableSet = new MultiAction<TInteractable>();

	private MultiAction<TInteractable> _whenInteractableUnset = new MultiAction<TInteractable>();

	private MultiAction<TInteractable> _whenInteractableSelected = new MultiAction<TInteractable>();

	private MultiAction<TInteractable> _whenInteractableUnselected = new MultiAction<TInteractable>();

	private UniqueIdentifier _identifier;

	[Tooltip("Can supply additional data (ex. data from an Interactable about a given Interactor, or vice-versa), or pass data along with events like PointerEvent (ex. the associated Interactor generating the event).")]
	[SerializeField]
	[Optional]
	private UnityEngine.Object _data;

	protected bool _started;

	public virtual bool ShouldHover
	{
		get
		{
			if (State != 0)
			{
				return false;
			}
			if (!HasCandidate)
			{
				return ComputeShouldSelect();
			}
			return true;
		}
	}

	public virtual bool ShouldUnhover
	{
		get
		{
			if (State != InteractorState.Hover)
			{
				return false;
			}
			if (!(_interactable != _candidate))
			{
				return _candidate == null;
			}
			return true;
		}
	}

	public bool ShouldSelect
	{
		get
		{
			if (State != InteractorState.Hover)
			{
				return false;
			}
			if (_computeShouldSelectOverride != null)
			{
				return _computeShouldSelectOverride();
			}
			if (_candidate == _interactable)
			{
				return ComputeShouldSelect();
			}
			return false;
		}
	}

	public bool ShouldUnselect
	{
		get
		{
			if (State != InteractorState.Select)
			{
				return false;
			}
			if (_computeShouldUnselectOverride != null)
			{
				return _computeShouldUnselectOverride();
			}
			return ComputeShouldUnselect();
		}
	}

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

	protected ISelector Selector
	{
		get
		{
			return _selector;
		}
		set
		{
			if (value != _selector && _selector != null && _started)
			{
				_selector.WhenSelected -= HandleSelected;
				_selector.WhenUnselected -= HandleUnselected;
			}
			_selector = value;
			if (_selector != null && _started)
			{
				_selector.WhenSelected += HandleSelected;
				_selector.WhenUnselected += HandleUnselected;
			}
		}
	}

	private bool QueuedSelect
	{
		get
		{
			if (_selectorQueue.Count > 0)
			{
				return _selectorQueue.Peek();
			}
			return false;
		}
	}

	private bool QueuedUnselect
	{
		get
		{
			if (_selectorQueue.Count > 0)
			{
				return !_selectorQueue.Peek();
			}
			return false;
		}
	}

	public InteractorState State
	{
		get
		{
			return _state;
		}
		private set
		{
			if (_state != value)
			{
				InteractorState state = _state;
				_state = value;
				this.WhenStateChanged(new InteractorStateChangeArgs(state, _state));
				if (_nativeId != 5282254251404903456L && _state == InteractorState.Select)
				{
					NativeMethods.isdk_NativeComponent_Activate(_nativeId);
				}
			}
		}
	}

	public virtual object CandidateProperties => null;

	public TInteractable Candidate => _candidate;

	public TInteractable Interactable => _interactable;

	public TInteractable SelectedInteractable => _selectedInteractable;

	public bool HasCandidate => _candidate != null;

	public bool HasInteractable => _interactable != null;

	public bool HasSelectedInteractable => _selectedInteractable != null;

	public MAction<TInteractable> WhenInteractableSet => _whenInteractableSet;

	public MAction<TInteractable> WhenInteractableUnset => _whenInteractableUnset;

	public MAction<TInteractable> WhenInteractableSelected => _whenInteractableSelected;

	public MAction<TInteractable> WhenInteractableUnselected => _whenInteractableUnselected;

	public int Identifier => _identifier.ID;

	public object Data { get; protected set; }

	public bool IsRootDriver { get; set; } = true;


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

	protected virtual void DoPreprocess()
	{
	}

	protected virtual void DoNormalUpdate()
	{
	}

	protected virtual void DoHoverUpdate()
	{
	}

	protected virtual void DoSelectUpdate()
	{
	}

	protected virtual void DoPostprocess()
	{
	}

	protected virtual bool ComputeShouldSelect()
	{
		return QueuedSelect;
	}

	protected virtual bool ComputeShouldUnselect()
	{
		return QueuedUnselect;
	}

	protected virtual void InteractableSet(TInteractable interactable)
	{
		_whenInteractableSet.Invoke(interactable);
	}

	protected virtual void InteractableUnset(TInteractable interactable)
	{
		_whenInteractableUnset.Invoke(interactable);
	}

	protected virtual void InteractableSelected(TInteractable interactable)
	{
		_whenInteractableSelected.Invoke(interactable);
	}

	protected virtual void InteractableUnselected(TInteractable interactable)
	{
		_whenInteractableUnselected.Invoke(interactable);
	}

	protected virtual void Awake()
	{
		_identifier = UniqueIdentifier.Generate(Context.Global.GetInstance(), this);
		ActiveState = _activeState as IActiveState;
		CandidateTiebreaker = _candidateTiebreaker as IComparer<TInteractable>;
		InteractableFilters = _interactableFilters.ConvertAll((UnityEngine.Object mono) => mono as IGameObjectFilter);
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		if (Data == null)
		{
			if (_data == null)
			{
				_data = this;
			}
			Data = _data;
		}
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started && _selector != null)
		{
			_selectorQueue.Clear();
			_selector.WhenSelected += HandleSelected;
			_selector.WhenUnselected += HandleUnselected;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			if (_selector != null)
			{
				_selector.WhenSelected -= HandleSelected;
				_selector.WhenUnselected -= HandleUnselected;
			}
			Disable();
		}
	}

	protected virtual void OnDestroy()
	{
		UniqueIdentifier.Release(_identifier);
	}

	public virtual void SetComputeCandidateOverride(Func<TInteractable> computeCandidate, bool shouldClearOverrideOnSelect = true)
	{
		_computeCandidateOverride = computeCandidate;
		_clearComputeCandidateOverrideOnSelect = shouldClearOverrideOnSelect;
	}

	public virtual void ClearComputeCandidateOverride()
	{
		_computeCandidateOverride = null;
		_clearComputeCandidateOverrideOnSelect = false;
	}

	public virtual void SetComputeShouldSelectOverride(Func<bool> computeShouldSelect, bool clearOverrideOnSelect = true)
	{
		_computeShouldSelectOverride = computeShouldSelect;
		_clearComputeShouldSelectOverrideOnSelect = clearOverrideOnSelect;
	}

	public virtual void ClearComputeShouldSelectOverride()
	{
		_computeShouldSelectOverride = null;
		_clearComputeShouldSelectOverrideOnSelect = false;
	}

	public virtual void SetComputeShouldUnselectOverride(Func<bool> computeShouldUnselect, bool clearOverrideOnUnselect = true)
	{
		_computeShouldUnselectOverride = computeShouldUnselect;
		_clearComputeShouldUnselectOverrideOnUnselect = clearOverrideOnUnselect;
	}

	public virtual void ClearComputeShouldUnselectOverride()
	{
		_computeShouldUnselectOverride = null;
		_clearComputeShouldUnselectOverrideOnUnselect = false;
	}

	public void Preprocess()
	{
		if (_started)
		{
			DoPreprocess();
		}
		if (!UpdateActiveState())
		{
			Disable();
		}
		this.WhenPreprocessed();
	}

	public void Process()
	{
		switch (State)
		{
		case InteractorState.Normal:
			DoNormalUpdate();
			break;
		case InteractorState.Hover:
			DoHoverUpdate();
			break;
		case InteractorState.Select:
			DoSelectUpdate();
			break;
		}
		this.WhenProcessed();
	}

	public void Postprocess()
	{
		_selectorQueue.Clear();
		if (_started)
		{
			DoPostprocess();
		}
		this.WhenPostprocessed();
	}

	public virtual void ProcessCandidate()
	{
		_candidate = null;
		if (UpdateActiveState())
		{
			if (_computeCandidateOverride != null)
			{
				_candidate = _computeCandidateOverride();
			}
			else
			{
				_candidate = ComputeCandidate();
			}
		}
	}

	public void InteractableChangesUpdate()
	{
		if (_selectedInteractable != null && !_selectedInteractable.HasSelectingInteractor(this as TInteractor))
		{
			UnselectInteractable();
		}
		if (_interactable != null && !_interactable.HasInteractor(this as TInteractor))
		{
			UnsetInteractable();
		}
	}

	public void Hover()
	{
		if (State == InteractorState.Normal)
		{
			SetInteractable(_candidate);
			State = InteractorState.Hover;
		}
	}

	public void Unhover()
	{
		if (State == InteractorState.Hover)
		{
			UnsetInteractable();
			State = InteractorState.Normal;
		}
	}

	public virtual void Select()
	{
		if (State == InteractorState.Hover)
		{
			if (_clearComputeCandidateOverrideOnSelect)
			{
				ClearComputeCandidateOverride();
			}
			if (_clearComputeShouldSelectOverrideOnSelect)
			{
				ClearComputeShouldSelectOverride();
			}
			while (QueuedSelect)
			{
				_selectorQueue.Dequeue();
			}
			if (Interactable != null)
			{
				SelectInteractable(Interactable);
			}
			State = InteractorState.Select;
		}
	}

	public virtual void Unselect()
	{
		if (State == InteractorState.Select)
		{
			if (_clearComputeShouldUnselectOverrideOnUnselect)
			{
				ClearComputeShouldUnselectOverride();
			}
			while (QueuedUnselect)
			{
				_selectorQueue.Dequeue();
			}
			UnselectInteractable();
			State = InteractorState.Hover;
		}
	}

	protected abstract TInteractable ComputeCandidate();

	protected virtual int ComputeCandidateTiebreaker(TInteractable a, TInteractable b)
	{
		if (CandidateTiebreaker == null)
		{
			return 0;
		}
		return CandidateTiebreaker.Compare(a, b);
	}

	public virtual bool CanSelect(TInteractable interactable)
	{
		if (InteractableFilters == null)
		{
			return true;
		}
		foreach (IGameObjectFilter interactableFilter in InteractableFilters)
		{
			if (!interactableFilter.Filter(interactable.gameObject))
			{
				return false;
			}
		}
		return true;
	}

	private void SetInteractable(TInteractable interactable)
	{
		if (!(_interactable == interactable))
		{
			UnsetInteractable();
			_interactable = interactable;
			interactable.AddInteractor(this as TInteractor);
			InteractableSet(interactable);
		}
	}

	private void UnsetInteractable()
	{
		TInteractable interactable = _interactable;
		if (!(interactable == null))
		{
			_interactable = null;
			interactable.RemoveInteractor(this as TInteractor);
			InteractableUnset(interactable);
		}
	}

	private void SelectInteractable(TInteractable interactable)
	{
		Unselect();
		_selectedInteractable = interactable;
		interactable.AddSelectingInteractor(this as TInteractor);
		InteractableSelected(interactable);
	}

	private void UnselectInteractable()
	{
		TInteractable selectedInteractable = _selectedInteractable;
		if (!(selectedInteractable == null))
		{
			_selectedInteractable = null;
			selectedInteractable.RemoveSelectingInteractor(this as TInteractor);
			InteractableUnselected(selectedInteractable);
		}
	}

	public void Enable()
	{
		if (UpdateActiveState() && State == InteractorState.Disabled)
		{
			State = InteractorState.Normal;
			HandleEnabled();
		}
	}

	public void Disable()
	{
		if (State != InteractorState.Disabled)
		{
			HandleDisabled();
			if (State == InteractorState.Select)
			{
				UnselectInteractable();
				State = InteractorState.Hover;
			}
			if (State == InteractorState.Hover)
			{
				UnsetInteractable();
				State = InteractorState.Normal;
			}
			if (State == InteractorState.Normal)
			{
				State = InteractorState.Disabled;
			}
		}
	}

	protected virtual void HandleEnabled()
	{
	}

	protected virtual void HandleDisabled()
	{
	}

	protected virtual void HandleSelected()
	{
		_selectorQueue.Enqueue(item: true);
	}

	protected virtual void HandleUnselected()
	{
		_selectorQueue.Enqueue(item: false);
	}

	private bool UpdateActiveState()
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
		InteractorState state = State;
		for (int i = 0; i < MaxIterationsPerFrame; i++)
		{
			if (State == InteractorState.Normal || (State == InteractorState.Hover && state != 0))
			{
				ProcessCandidate();
			}
			state = State;
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

	public void InjectOptionalActiveState(IActiveState activeState)
	{
		_activeState = activeState as UnityEngine.Object;
		ActiveState = activeState;
	}

	public void InjectOptionalInteractableFilters(List<IGameObjectFilter> interactableFilters)
	{
		InteractableFilters = interactableFilters;
		_interactableFilters = interactableFilters.ConvertAll((IGameObjectFilter interactableFilter) => interactableFilter as UnityEngine.Object);
	}

	public void InjectOptionalCandidateTiebreaker(IComparer<TInteractable> candidateTiebreaker)
	{
		_candidateTiebreaker = candidateTiebreaker as UnityEngine.Object;
		CandidateTiebreaker = candidateTiebreaker;
	}

	public void InjectOptionalData(object data)
	{
		_data = data as UnityEngine.Object;
		Data = data;
	}
}
