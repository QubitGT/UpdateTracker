using System;
using System.Collections.Generic;
using System.Linq;
using Oculus.Interaction.Collections;
using UnityEngine;

namespace Oculus.Interaction;

public abstract class Interactable<TInteractor, TInteractable> : MonoBehaviour, IInteractable, IInteractableView where TInteractor : Interactor<TInteractor, TInteractable> where TInteractable : Interactable<TInteractor, TInteractable>
{
	[SerializeField]
	[Interface(typeof(IGameObjectFilter), new Type[] { })]
	[Optional]
	private List<UnityEngine.Object> _interactorFilters = new List<UnityEngine.Object>();

	private List<IGameObjectFilter> InteractorFilters;

	[SerializeField]
	private int _maxInteractors = -1;

	[SerializeField]
	private int _maxSelectingInteractors = -1;

	[SerializeField]
	[Optional]
	private UnityEngine.Object _data;

	protected bool _started;

	private EnumerableHashSet<TInteractor> _interactors = new EnumerableHashSet<TInteractor>();

	private EnumerableHashSet<TInteractor> _selectingInteractors = new EnumerableHashSet<TInteractor>();

	private InteractableState _state = InteractableState.Disabled;

	private MultiAction<TInteractor> _whenInteractorAdded = new MultiAction<TInteractor>();

	private MultiAction<TInteractor> _whenInteractorRemoved = new MultiAction<TInteractor>();

	private MultiAction<TInteractor> _whenSelectingInteractorAdded = new MultiAction<TInteractor>();

	private MultiAction<TInteractor> _whenSelectingInteractorRemoved = new MultiAction<TInteractor>();

	private static InteractableRegistry<TInteractor, TInteractable> _registry = new InteractableRegistry<TInteractor, TInteractable>();

	public object Data { get; protected set; }

	public int MaxInteractors
	{
		get
		{
			return _maxInteractors;
		}
		set
		{
			_maxInteractors = value;
		}
	}

	public int MaxSelectingInteractors
	{
		get
		{
			return _maxSelectingInteractors;
		}
		set
		{
			_maxSelectingInteractors = value;
		}
	}

	public IEnumerable<IInteractorView> InteractorViews => _interactors.Cast<IInteractorView>();

	public IEnumerable<IInteractorView> SelectingInteractorViews => _selectingInteractors.Cast<IInteractorView>();

	public MAction<TInteractor> WhenInteractorAdded => _whenInteractorAdded;

	public MAction<TInteractor> WhenInteractorRemoved => _whenInteractorRemoved;

	public MAction<TInteractor> WhenSelectingInteractorAdded => _whenSelectingInteractorAdded;

	public MAction<TInteractor> WhenSelectingInteractorRemoved => _whenSelectingInteractorRemoved;

	public InteractableState State
	{
		get
		{
			return _state;
		}
		private set
		{
			if (_state != value)
			{
				InteractableState state = _state;
				_state = value;
				this.WhenStateChanged(new InteractableStateChangeArgs(state, _state));
			}
		}
	}

	public static InteractableRegistry<TInteractor, TInteractable> Registry => _registry;

	public IEnumerableHashSet<TInteractor> Interactors => _interactors;

	public IEnumerableHashSet<TInteractor> SelectingInteractors => _selectingInteractors;

	public event Action<InteractableStateChangeArgs> WhenStateChanged = delegate
	{
	};

	public event Action<IInteractorView> WhenInteractorViewAdded = delegate
	{
	};

	public event Action<IInteractorView> WhenInteractorViewRemoved = delegate
	{
	};

	public event Action<IInteractorView> WhenSelectingInteractorViewAdded = delegate
	{
	};

	public event Action<IInteractorView> WhenSelectingInteractorViewRemoved = delegate
	{
	};

	protected virtual void InteractorAdded(TInteractor interactor)
	{
		this.WhenInteractorViewAdded(interactor);
		_whenInteractorAdded.Invoke(interactor);
	}

	protected virtual void InteractorRemoved(TInteractor interactor)
	{
		this.WhenInteractorViewRemoved(interactor);
		_whenInteractorRemoved.Invoke(interactor);
	}

	protected virtual void SelectingInteractorAdded(TInteractor interactor)
	{
		this.WhenSelectingInteractorViewAdded(interactor);
		_whenSelectingInteractorAdded.Invoke(interactor);
	}

	protected virtual void SelectingInteractorRemoved(TInteractor interactor)
	{
		this.WhenSelectingInteractorViewRemoved(interactor);
		_whenSelectingInteractorRemoved.Invoke(interactor);
	}

	public void AddInteractor(TInteractor interactor)
	{
		_interactors.Add(interactor);
		InteractorAdded(interactor);
		UpdateInteractableState();
	}

	public void RemoveInteractor(TInteractor interactor)
	{
		if (_interactors.Remove(interactor))
		{
			interactor.InteractableChangesUpdate();
			InteractorRemoved(interactor);
			UpdateInteractableState();
		}
	}

	public void AddSelectingInteractor(TInteractor interactor)
	{
		_selectingInteractors.Add(interactor);
		SelectingInteractorAdded(interactor);
		UpdateInteractableState();
	}

	public void RemoveSelectingInteractor(TInteractor interactor)
	{
		if (_selectingInteractors.Remove(interactor))
		{
			interactor.InteractableChangesUpdate();
			SelectingInteractorRemoved(interactor);
			UpdateInteractableState();
		}
	}

	private void UpdateInteractableState()
	{
		if (State != InteractableState.Disabled)
		{
			if (_selectingInteractors.Count > 0)
			{
				State = InteractableState.Select;
			}
			else if (_interactors.Count > 0)
			{
				State = InteractableState.Hover;
			}
			else
			{
				State = InteractableState.Normal;
			}
		}
	}

	public bool CanBeSelectedBy(TInteractor interactor)
	{
		if (State == InteractableState.Disabled)
		{
			return false;
		}
		if (MaxSelectingInteractors >= 0 && _selectingInteractors.Count == MaxSelectingInteractors)
		{
			return false;
		}
		if (MaxInteractors >= 0 && _interactors.Count == MaxInteractors && !_interactors.Contains(interactor))
		{
			return false;
		}
		if (InteractorFilters == null)
		{
			return true;
		}
		foreach (IGameObjectFilter interactorFilter in InteractorFilters)
		{
			if (!interactorFilter.Filter(interactor.gameObject))
			{
				return false;
			}
		}
		return true;
	}

	public bool HasInteractor(TInteractor interactor)
	{
		return _interactors.Contains(interactor);
	}

	public bool HasSelectingInteractor(TInteractor interactor)
	{
		return _selectingInteractors.Contains(interactor);
	}

	public void Enable()
	{
		if (State == InteractableState.Disabled && _started)
		{
			_registry.Register((TInteractable)this);
			State = InteractableState.Normal;
		}
	}

	public void Disable()
	{
		if (State == InteractableState.Disabled || !_started)
		{
			return;
		}
		foreach (TInteractor item in new List<TInteractor>(_selectingInteractors))
		{
			RemoveSelectingInteractor(item);
		}
		foreach (TInteractor item2 in new List<TInteractor>(_interactors))
		{
			RemoveInteractor(item2);
		}
		_registry.Unregister((TInteractable)this);
		State = InteractableState.Disabled;
	}

	public void RemoveInteractorByIdentifier(int id)
	{
		TInteractor val = null;
		foreach (TInteractor selectingInteractor in _selectingInteractors)
		{
			if (selectingInteractor.Identifier == id)
			{
				val = selectingInteractor;
				break;
			}
		}
		if (val != null)
		{
			RemoveSelectingInteractor(val);
		}
		val = null;
		foreach (TInteractor interactor in _interactors)
		{
			if (interactor.Identifier == id)
			{
				val = interactor;
				break;
			}
		}
		if (!(val == null))
		{
			RemoveInteractor(val);
		}
	}

	protected virtual void Awake()
	{
		InteractorFilters = _interactorFilters.ConvertAll((UnityEngine.Object mono) => mono as IGameObjectFilter);
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
		Enable();
	}

	protected virtual void OnDisable()
	{
		Disable();
	}

	protected virtual void SetRegistry(InteractableRegistry<TInteractor, TInteractable> registry)
	{
		if (registry == _registry)
		{
			return;
		}
		foreach (TInteractable item in _registry.List())
		{
			registry.Register(item);
			_registry.Unregister(item);
		}
		_registry = registry;
	}

	public void InjectOptionalInteractorFilters(List<IGameObjectFilter> interactorFilters)
	{
		InteractorFilters = interactorFilters;
		_interactorFilters = interactorFilters.ConvertAll((IGameObjectFilter interactorFilter) => interactorFilter as UnityEngine.Object);
	}

	public void InjectOptionalData(object data)
	{
		_data = data as UnityEngine.Object;
		Data = data;
	}
}
