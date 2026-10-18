using System;
using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction;

public class InteractorUnityEventWrapper : MonoBehaviour
{
	[Tooltip("The IInteractorView (Interactor) component to wrap.")]
	[SerializeField]
	[Interface(typeof(IInteractorView), new Type[] { })]
	private UnityEngine.Object _interactorView;

	private IInteractorView InteractorView;

	[Tooltip("Raised when the Interactor is enabled.")]
	[SerializeField]
	private UnityEvent _whenEnabled;

	[Tooltip("Raised when the Interactor is disabled.")]
	[SerializeField]
	private UnityEvent _whenDisabled;

	[Tooltip("Raised when the Interactor is hovering over an Interactable.")]
	[SerializeField]
	private UnityEvent _whenHover;

	[Tooltip("Raised when the stops hovering over an Interactable.")]
	[SerializeField]
	private UnityEvent _whenUnhover;

	[Tooltip("Raised when the Interactor selects an Interactable.")]
	[SerializeField]
	private UnityEvent _whenSelect;

	[Tooltip("Raised when the Interactor stops selecting an Interactable.")]
	[SerializeField]
	private UnityEvent _whenUnselect;

	[Space]
	[Tooltip("Raised when the Interactor preprocesses.")]
	[SerializeField]
	private UnityEvent _whenPreprocessed;

	[Tooltip("Raised when the Interactor processes.")]
	[SerializeField]
	private UnityEvent _whenProcessed;

	[Tooltip("Raised when the Interactor processes.")]
	[SerializeField]
	private UnityEvent _whenPostprocessed;

	protected bool _started;

	public UnityEvent WhenDisabled => _whenDisabled;

	public UnityEvent WhenEnabled => _whenEnabled;

	public UnityEvent WhenHover => _whenHover;

	public UnityEvent WhenUnhover => _whenUnhover;

	public UnityEvent WhenSelect => _whenSelect;

	public UnityEvent WhenUnselect => _whenUnselect;

	public UnityEvent WhenPreprocessed => _whenPreprocessed;

	public UnityEvent WhenProcessed => _whenProcessed;

	public UnityEvent WhenPostprocessed => _whenPostprocessed;

	protected virtual void Awake()
	{
		InteractorView = _interactorView as IInteractorView;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			InteractorView.WhenStateChanged += HandleStateChanged;
			InteractorView.WhenPreprocessed += HandlePreprocessed;
			InteractorView.WhenProcessed += HandleProcessed;
			InteractorView.WhenPostprocessed += HandlePostprocessed;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			InteractorView.WhenStateChanged -= HandleStateChanged;
			InteractorView.WhenPreprocessed -= HandlePreprocessed;
			InteractorView.WhenProcessed -= HandleProcessed;
			InteractorView.WhenPostprocessed -= HandlePostprocessed;
		}
	}

	private void HandleStateChanged(InteractorStateChangeArgs args)
	{
		switch (args.NewState)
		{
		case InteractorState.Disabled:
			_whenDisabled.Invoke();
			break;
		case InteractorState.Normal:
			if (args.PreviousState == InteractorState.Hover)
			{
				_whenUnhover.Invoke();
			}
			else if (args.PreviousState == InteractorState.Disabled)
			{
				_whenEnabled.Invoke();
			}
			break;
		case InteractorState.Hover:
			if (args.PreviousState == InteractorState.Normal)
			{
				_whenHover.Invoke();
			}
			else if (args.PreviousState == InteractorState.Select)
			{
				_whenUnselect.Invoke();
			}
			break;
		case InteractorState.Select:
			if (args.PreviousState == InteractorState.Hover)
			{
				_whenSelect.Invoke();
			}
			break;
		}
	}

	private void HandlePreprocessed()
	{
		_whenPreprocessed.Invoke();
	}

	private void HandleProcessed()
	{
		_whenProcessed.Invoke();
	}

	private void HandlePostprocessed()
	{
		_whenPostprocessed.Invoke();
	}

	public void InjectAllInteractorUnityEventWrapper(IInteractorView interactorView)
	{
		InjectInteractorView(interactorView);
	}

	public void InjectInteractorView(IInteractorView interactorView)
	{
		_interactorView = interactorView as UnityEngine.Object;
		InteractorView = interactorView;
	}
}
