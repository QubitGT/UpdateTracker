using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Interactables;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRSelectInteractable : IXRInteractable
{
	SelectEnterEvent firstSelectEntered { get; }

	SelectExitEvent lastSelectExited { get; }

	SelectEnterEvent selectEntered { get; }

	SelectExitEvent selectExited { get; }

	List<IXRSelectInteractor> interactorsSelecting { get; }

	IXRSelectInteractor firstInteractorSelecting { get; }

	bool isSelected { get; }

	InteractableSelectMode selectMode { get; }

	bool IsSelectableBy(IXRSelectInteractor interactor);

	Pose GetAttachPoseOnSelect(IXRSelectInteractor interactor);

	Pose GetLocalAttachPoseOnSelect(IXRSelectInteractor interactor);

	void OnSelectEntering(SelectEnterEventArgs args);

	void OnSelectEntered(SelectEnterEventArgs args);

	void OnSelectExiting(SelectExitEventArgs args);

	void OnSelectExited(SelectExitEventArgs args);
}
