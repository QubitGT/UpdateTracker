using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Interactables;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRFocusInteractable : IXRInteractable
{
	FocusEnterEvent firstFocusEntered { get; }

	FocusExitEvent lastFocusExited { get; }

	FocusEnterEvent focusEntered { get; }

	FocusExitEvent focusExited { get; }

	List<IXRInteractionGroup> interactionGroupsFocusing { get; }

	IXRInteractionGroup firstInteractionGroupFocusing { get; }

	bool isFocused { get; }

	InteractableFocusMode focusMode { get; }

	bool canFocus { get; }

	void OnFocusEntering(FocusEnterEventArgs args);

	void OnFocusEntered(FocusEnterEventArgs args);

	void OnFocusExiting(FocusExitEventArgs args);

	void OnFocusExited(FocusExitEventArgs args);
}
