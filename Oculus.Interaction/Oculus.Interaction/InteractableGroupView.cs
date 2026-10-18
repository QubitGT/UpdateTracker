using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Oculus.Interaction;

public class InteractableGroupView : MonoBehaviour, IInteractableView
{
	[SerializeField]
	[Interface(typeof(IInteractableView), new Type[] { })]
	private List<UnityEngine.Object> _interactables;

	private List<IInteractableView> Interactables;

	[SerializeField]
	[Optional]
	private UnityEngine.Object _data;

	private InteractableState _state;

	protected bool _started;

	public object Data { get; protected set; }

	public int InteractorsCount
	{
		get
		{
			int num = 0;
			foreach (IInteractableView interactable in Interactables)
			{
				num += interactable.InteractorViews.Count();
			}
			return num;
		}
	}

	public int SelectingInteractorsCount
	{
		get
		{
			int num = 0;
			foreach (IInteractableView interactable in Interactables)
			{
				num += interactable.SelectingInteractorViews.Count();
			}
			return num;
		}
	}

	public IEnumerable<IInteractorView> InteractorViews => Interactables.SelectMany((IInteractableView interactable) => interactable.InteractorViews).ToList();

	public IEnumerable<IInteractorView> SelectingInteractorViews => Interactables.SelectMany((IInteractableView interactable) => interactable.SelectingInteractorViews).ToList();

	public int MaxInteractors
	{
		get
		{
			int num = 0;
			foreach (IInteractableView interactable in Interactables)
			{
				num = Mathf.Max(interactable.MaxInteractors, num);
			}
			return num;
		}
	}

	public int MaxSelectingInteractors
	{
		get
		{
			int num = 0;
			foreach (IInteractableView interactable in Interactables)
			{
				num = Mathf.Max(interactable.MaxSelectingInteractors, num);
			}
			return num;
		}
	}

	public InteractableState State
	{
		get
		{
			return _state;
		}
		set
		{
			if (_state != value)
			{
				InteractableState state = _state;
				_state = value;
				this.WhenStateChanged(new InteractableStateChangeArgs(state, _state));
			}
		}
	}

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

	public event Action<InteractableStateChangeArgs> WhenStateChanged = delegate
	{
	};

	private void UpdateState()
	{
		if (SelectingInteractorsCount > 0)
		{
			State = InteractableState.Select;
		}
		else if (InteractorsCount > 0)
		{
			State = InteractableState.Hover;
		}
		else
		{
			State = InteractableState.Normal;
		}
	}

	protected virtual void Awake()
	{
		if (_interactables != null)
		{
			Interactables = _interactables.ConvertAll((UnityEngine.Object mono) => mono as IInteractableView);
		}
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		if (Data == null)
		{
			_data = this;
			Data = _data;
		}
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (!_started)
		{
			return;
		}
		foreach (IInteractableView interactable in Interactables)
		{
			interactable.WhenStateChanged += HandleStateChange;
			interactable.WhenInteractorViewAdded += HandleInteractorViewAdded;
			interactable.WhenInteractorViewRemoved += HandleInteractorViewRemoved;
			interactable.WhenSelectingInteractorViewAdded += HandleSelectingInteractorViewAdded;
			interactable.WhenSelectingInteractorViewRemoved += HandleSelectingInteractorViewRemoved;
		}
	}

	protected virtual void OnDisable()
	{
		if (!_started)
		{
			return;
		}
		foreach (IInteractableView interactable in Interactables)
		{
			interactable.WhenStateChanged -= HandleStateChange;
			interactable.WhenInteractorViewAdded -= HandleInteractorViewAdded;
			interactable.WhenInteractorViewRemoved -= HandleInteractorViewRemoved;
			interactable.WhenSelectingInteractorViewAdded -= HandleSelectingInteractorViewAdded;
			interactable.WhenSelectingInteractorViewRemoved -= HandleSelectingInteractorViewRemoved;
		}
	}

	private void HandleStateChange(InteractableStateChangeArgs args)
	{
		UpdateState();
	}

	private void HandleInteractorViewAdded(IInteractorView obj)
	{
		this.WhenInteractorViewAdded(obj);
	}

	private void HandleInteractorViewRemoved(IInteractorView obj)
	{
		this.WhenInteractorViewRemoved(obj);
	}

	private void HandleSelectingInteractorViewAdded(IInteractorView obj)
	{
		this.WhenSelectingInteractorViewAdded(obj);
	}

	private void HandleSelectingInteractorViewRemoved(IInteractorView obj)
	{
		this.WhenSelectingInteractorViewRemoved(obj);
	}

	public void InjectAllInteractableGroupView(List<IInteractableView> interactables)
	{
		InjectInteractables(interactables);
	}

	public void InjectInteractables(List<IInteractableView> interactables)
	{
		Interactables = interactables;
		_interactables = Interactables.ConvertAll((IInteractableView interactable) => interactable as UnityEngine.Object);
	}

	public void InjectOptionalData(object data)
	{
		_data = data as UnityEngine.Object;
		Data = data;
	}
}
