using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRSelectInteractor : IXRInteractor
{
	SelectEnterEvent selectEntered { get; }

	SelectExitEvent selectExited { get; }

	List<IXRSelectInteractable> interactablesSelected { get; }

	IXRSelectInteractable firstInteractableSelected { get; }

	bool hasSelection { get; }

	bool isSelectActive { get; }

	bool keepSelectedTargetValid { get; }

	bool CanSelect(IXRSelectInteractable interactable);

	bool IsSelecting(IXRSelectInteractable interactable);

	Pose GetAttachPoseOnSelect(IXRSelectInteractable interactable);

	Pose GetLocalAttachPoseOnSelect(IXRSelectInteractable interactable);

	void OnSelectEntering(SelectEnterEventArgs args);

	void OnSelectEntered(SelectEnterEventArgs args);

	void OnSelectExiting(SelectExitEventArgs args);

	void OnSelectExited(SelectExitEventArgs args);
}
