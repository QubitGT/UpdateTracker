using UnityEngine;

namespace Oculus.Interaction;

public abstract class PointerInteractor<TInteractor, TInteractable> : Interactor<TInteractor, TInteractable> where TInteractor : Interactor<TInteractor, TInteractable> where TInteractable : PointerInteractable<TInteractor, TInteractable>
{
	protected void GeneratePointerEvent(PointerEventType pointerEventType, TInteractable interactable)
	{
		Pose pose = ComputePointerPose();
		if (interactable == null)
		{
			return;
		}
		if (interactable.PointableElement != null)
		{
			switch (pointerEventType)
			{
			case PointerEventType.Hover:
				interactable.PointableElement.WhenPointerEventRaised += HandlePointerEventRaised;
				break;
			case PointerEventType.Unhover:
				interactable.PointableElement.WhenPointerEventRaised -= HandlePointerEventRaised;
				break;
			}
		}
		interactable.PublishPointerEvent(new PointerEvent(base.Identifier, pointerEventType, pose, base.Data));
	}

	protected virtual void HandlePointerEventRaised(PointerEvent evt)
	{
		if (evt.Identifier == base.Identifier && evt.Type == PointerEventType.Cancel && base.Interactable != null)
		{
			TInteractable interactable = base.Interactable;
			interactable.RemoveInteractorByIdentifier(base.Identifier);
			interactable.PointableElement.WhenPointerEventRaised -= HandlePointerEventRaised;
		}
	}

	protected override void InteractableSet(TInteractable interactable)
	{
		base.InteractableSet(interactable);
		GeneratePointerEvent(PointerEventType.Hover, interactable);
	}

	protected override void InteractableUnset(TInteractable interactable)
	{
		GeneratePointerEvent(PointerEventType.Unhover, interactable);
		base.InteractableUnset(interactable);
	}

	protected override void InteractableSelected(TInteractable interactable)
	{
		base.InteractableSelected(interactable);
		GeneratePointerEvent(PointerEventType.Select, interactable);
	}

	protected override void InteractableUnselected(TInteractable interactable)
	{
		GeneratePointerEvent(PointerEventType.Unselect, interactable);
		base.InteractableUnselected(interactable);
	}

	protected override void DoPostprocess()
	{
		base.DoPostprocess();
		if (_interactable != null)
		{
			GeneratePointerEvent(PointerEventType.Move, _interactable);
		}
	}

	protected abstract Pose ComputePointerPose();
}
