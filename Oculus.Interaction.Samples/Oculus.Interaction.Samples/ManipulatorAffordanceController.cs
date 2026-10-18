using Oculus.Interaction.HandGrab;
using UnityEngine;

namespace Oculus.Interaction.Samples;

public class ManipulatorAffordanceController : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The grab interactable for the slate itself (as opposed to the surrounding affordances)")]
	private GrabInteractable _grabInteractable;

	[SerializeField]
	[Tooltip("The hand grab interactable for the slate itself (as opposed to the surrounding affordances)")]
	private HandGrabInteractable _handGrabInteractable;

	[SerializeField]
	[Optional]
	[Tooltip("The ray interactable for the slate itself (as opposed to the surrounding affordances)")]
	private RayInteractable _rayInteractable;

	[SerializeField]
	[Tooltip("The state signaler for the SlateWithManipulators prefab")]
	private PanelWithManipulatorsStateSignaler _stateSignaler;

	[SerializeField]
	[Tooltip("The animators (canonically geometry and opacity) whose 'state' variables should be controlled by this affordance")]
	private Animator[] _animators;

	[SerializeField]
	[Optional]
	[Tooltip("Holds the panel hover state")]
	private PanelHoverState _panelHoverState;

	private void Start()
	{
		int animatorState = GetAnimatorState();
		Animator[] animators = _animators;
		for (int i = 0; i < animators.Length; i++)
		{
			animators[i].SetInteger("state", animatorState);
		}
		_grabInteractable.WhenStateChanged += HandleInteractableStateChanged;
		_handGrabInteractable.WhenStateChanged += HandleInteractableStateChanged;
		if (_rayInteractable != null)
		{
			_rayInteractable.WhenStateChanged += HandleInteractableStateChanged;
		}
		_stateSignaler.WhenStateChanged += HandleStateChanged;
		if (_panelHoverState != null)
		{
			_panelHoverState.WhenStateChanged += PanelHoverStateChanged;
		}
	}

	private void OnDestroy()
	{
		_grabInteractable.WhenStateChanged -= HandleInteractableStateChanged;
		_handGrabInteractable.WhenStateChanged -= HandleInteractableStateChanged;
		if (_rayInteractable != null)
		{
			_rayInteractable.WhenStateChanged -= HandleInteractableStateChanged;
		}
		_stateSignaler.WhenStateChanged -= HandleStateChanged;
		if (_panelHoverState != null)
		{
			_panelHoverState.WhenStateChanged -= PanelHoverStateChanged;
		}
	}

	private int GetAnimatorStateFromInteractable(IInteractableView view)
	{
		int result = 0;
		switch (view.State)
		{
		case InteractableState.Normal:
			result = 1;
			break;
		case InteractableState.Hover:
			result = 2;
			break;
		case InteractableState.Select:
			result = 3;
			break;
		}
		return result;
	}

	private int GetAnimatorState()
	{
		int result = 0;
		if ((object)_panelHoverState != null && !_panelHoverState.Hovered)
		{
			return result;
		}
		int num = ((_rayInteractable != null) ? GetAnimatorStateFromInteractable(_rayInteractable) : 0);
		return Mathf.Max(GetAnimatorStateFromInteractable(_grabInteractable), GetAnimatorStateFromInteractable(_handGrabInteractable), num);
	}

	private void HandleInteractableStateChanged(InteractableStateChangeArgs args)
	{
		if (args.NewState == InteractableState.Select)
		{
			_stateSignaler.CurrentState = PanelWithManipulatorsStateSignaler.State.Selected;
		}
		else if (args.PreviousState == InteractableState.Select)
		{
			_stateSignaler.CurrentState = PanelWithManipulatorsStateSignaler.State.Default;
		}
		int animatorState = GetAnimatorState();
		Animator[] animators = _animators;
		for (int i = 0; i < animators.Length; i++)
		{
			animators[i].SetInteger("state", animatorState);
		}
	}

	private void PanelHoverStateChanged(bool newState)
	{
		int animatorState = GetAnimatorState();
		Animator[] animators = _animators;
		for (int i = 0; i < animators.Length; i++)
		{
			animators[i].SetInteger("state", animatorState);
		}
	}

	private void HandleStateChanged(PanelWithManipulatorsStateSignaler.State state)
	{
		if (state != 0)
		{
			bool flag = !(_rayInteractable != null) || _rayInteractable.State != InteractableState.Select;
			if (_grabInteractable.State != InteractableState.Select && _handGrabInteractable.State != InteractableState.Select && flag)
			{
				_grabInteractable.enabled = false;
				_handGrabInteractable.enabled = false;
				if (_rayInteractable != null)
				{
					_rayInteractable.enabled = false;
				}
			}
		}
		else
		{
			_grabInteractable.enabled = true;
			_handGrabInteractable.enabled = true;
			if (_rayInteractable != null)
			{
				_rayInteractable.enabled = true;
			}
		}
	}
}
