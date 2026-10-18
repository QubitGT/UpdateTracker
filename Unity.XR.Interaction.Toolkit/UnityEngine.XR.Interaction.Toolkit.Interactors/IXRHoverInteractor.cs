using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRHoverInteractor : IXRInteractor
{
	HoverEnterEvent hoverEntered { get; }

	HoverExitEvent hoverExited { get; }

	List<IXRHoverInteractable> interactablesHovered { get; }

	bool hasHover { get; }

	bool isHoverActive { get; }

	bool CanHover(IXRHoverInteractable interactable);

	bool IsHovering(IXRHoverInteractable interactable);

	void OnHoverEntering(HoverEnterEventArgs args);

	void OnHoverEntered(HoverEnterEventArgs args);

	void OnHoverExiting(HoverExitEventArgs args);

	void OnHoverExited(HoverExitEventArgs args);
}
