using UnityEngine;

namespace Oculus.Interaction.DistanceReticles;

public abstract class InteractorReticle<TReticleData> : MonoBehaviour where TReticleData : class, IReticleData
{
	[Tooltip("Should the reticle be visible when you're selecting an object?")]
	[SerializeField]
	private bool _visibleDuringSelect;

	protected bool _started;

	protected TReticleData _targetData;

	private bool _drawn;

	public bool VisibleDuringSelect
	{
		get
		{
			return _visibleDuringSelect;
		}
		set
		{
			_visibleDuringSelect = value;
		}
	}

	protected abstract IInteractorView Interactor { get; set; }

	protected abstract Component InteractableComponent { get; }

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		Hide();
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			Interactor.WhenStateChanged += HandleStateChanged;
			Interactor.WhenPostprocessed += HandlePostProcessed;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Interactor.WhenStateChanged -= HandleStateChanged;
			Interactor.WhenPostprocessed -= HandlePostProcessed;
		}
	}

	private void HandleStateChanged(InteractorStateChangeArgs args)
	{
		if (args.NewState == InteractorState.Normal || args.NewState == InteractorState.Disabled)
		{
			InteractableUnset();
		}
		else if (args.NewState == InteractorState.Hover && args.PreviousState != InteractorState.Select)
		{
			InteractableSet(InteractableComponent);
		}
	}

	private void HandlePostProcessed()
	{
		if (_targetData != null && (Interactor.State == InteractorState.Hover || (Interactor.State == InteractorState.Select && _visibleDuringSelect)))
		{
			if (!_drawn)
			{
				_drawn = true;
				Draw(_targetData);
			}
			Align(_targetData);
		}
		else if (_drawn)
		{
			_drawn = false;
			Hide();
		}
	}

	private void InteractableSet(Component interactable)
	{
		if (interactable != null && interactable.TryGetComponent<TReticleData>(out _targetData))
		{
			_drawn = false;
		}
		else
		{
			_targetData = null;
		}
	}

	private void InteractableUnset()
	{
		if (_drawn)
		{
			_drawn = false;
			Hide();
		}
		_targetData = null;
	}

	protected abstract void Draw(TReticleData data);

	protected abstract void Align(TReticleData data);

	protected abstract void Hide();
}
