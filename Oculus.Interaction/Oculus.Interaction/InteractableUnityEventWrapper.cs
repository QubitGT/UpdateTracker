using System;
using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction;

public class InteractableUnityEventWrapper : MonoBehaviour
{
	[Tooltip("The IInteractableView (Interactable) component to wrap.")]
	[SerializeField]
	[Interface(typeof(IInteractableView), new Type[] { })]
	private UnityEngine.Object _interactableView;

	private IInteractableView InteractableView;

	[Tooltip("Raised when an Interactor hovers over the Interactable.")]
	[SerializeField]
	private UnityEvent _whenHover;

	[Tooltip("Raised when the Interactable was being hovered but now it isn't.")]
	[SerializeField]
	private UnityEvent _whenUnhover;

	[Tooltip("Raised when an Interactor selects the Interactable.")]
	[SerializeField]
	private UnityEvent _whenSelect;

	[Tooltip("Raised when the Interactable was being selected but now it isn't.")]
	[SerializeField]
	private UnityEvent _whenUnselect;

	[Tooltip("Raised each time an Interactor hovers over the Interactable, even if the Interactable is already being hovered by a different Interactor.")]
	[SerializeField]
	private UnityEvent _whenInteractorViewAdded;

	[Tooltip("Raised each time an Interactor stops hovering over the Interactable, even if the Interactable is still being hovered by a different Interactor.")]
	[SerializeField]
	private UnityEvent _whenInteractorViewRemoved;

	[Tooltip("Raised each time an Interactor selects the Interactable, even if the Interactable is already being selected by a different Interactor.")]
	[SerializeField]
	private UnityEvent _whenSelectingInteractorViewAdded;

	[Tooltip("Raised each time an Interactor stops selecting the Interactable, even if the Interactable is still being selected by a different Interactor.")]
	[SerializeField]
	private UnityEvent _whenSelectingInteractorViewRemoved;

	protected bool _started;

	public UnityEvent WhenHover => _whenHover;

	public UnityEvent WhenUnhover => _whenUnhover;

	public UnityEvent WhenSelect => _whenSelect;

	public UnityEvent WhenUnselect => _whenUnselect;

	public UnityEvent WhenInteractorViewAdded => _whenInteractorViewAdded;

	public UnityEvent WhenInteractorViewRemoved => _whenInteractorViewRemoved;

	public UnityEvent WhenSelectingInteractorViewAdded => _whenSelectingInteractorViewAdded;

	public UnityEvent WhenSelectingInteractorViewRemoved => _whenSelectingInteractorViewRemoved;

	protected virtual void Awake()
	{
		InteractableView = _interactableView as IInteractableView;
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
			InteractableView.WhenStateChanged += HandleStateChanged;
			InteractableView.WhenInteractorViewAdded += HandleInteractorViewAdded;
			InteractableView.WhenInteractorViewRemoved += HandleInteractorViewRemoved;
			InteractableView.WhenSelectingInteractorViewAdded += HandleSelectingInteractorViewAdded;
			InteractableView.WhenSelectingInteractorViewRemoved += HandleSelectingInteractorViewRemoved;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			InteractableView.WhenStateChanged -= HandleStateChanged;
			InteractableView.WhenInteractorViewAdded -= HandleInteractorViewAdded;
			InteractableView.WhenInteractorViewRemoved -= HandleInteractorViewRemoved;
			InteractableView.WhenSelectingInteractorViewAdded -= HandleSelectingInteractorViewAdded;
			InteractableView.WhenSelectingInteractorViewRemoved -= HandleSelectingInteractorViewRemoved;
		}
	}

	private void HandleStateChanged(InteractableStateChangeArgs args)
	{
		switch (args.NewState)
		{
		case InteractableState.Normal:
			if (args.PreviousState == InteractableState.Hover)
			{
				_whenUnhover.Invoke();
			}
			break;
		case InteractableState.Hover:
			if (args.PreviousState == InteractableState.Normal)
			{
				_whenHover.Invoke();
			}
			else if (args.PreviousState == InteractableState.Select)
			{
				_whenUnselect.Invoke();
			}
			break;
		case InteractableState.Select:
			if (args.PreviousState == InteractableState.Hover)
			{
				_whenSelect.Invoke();
			}
			break;
		}
	}

	private void HandleInteractorViewAdded(IInteractorView interactorView)
	{
		WhenInteractorViewAdded.Invoke();
	}

	private void HandleInteractorViewRemoved(IInteractorView interactorView)
	{
		WhenInteractorViewRemoved.Invoke();
	}

	private void HandleSelectingInteractorViewAdded(IInteractorView interactorView)
	{
		WhenSelectingInteractorViewAdded.Invoke();
	}

	private void HandleSelectingInteractorViewRemoved(IInteractorView interactorView)
	{
		WhenSelectingInteractorViewRemoved.Invoke();
	}

	public void InjectAllInteractableUnityEventWrapper(IInteractableView interactableView)
	{
		InjectInteractableView(interactableView);
	}

	public void InjectInteractableView(IInteractableView interactableView)
	{
		_interactableView = interactableView as UnityEngine.Object;
		InteractableView = interactableView;
	}
}
