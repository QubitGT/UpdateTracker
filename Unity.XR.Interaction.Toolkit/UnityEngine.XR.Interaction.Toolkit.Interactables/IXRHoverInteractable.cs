using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Interactables;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRHoverInteractable : IXRInteractable
{
	HoverEnterEvent firstHoverEntered { get; }

	HoverExitEvent lastHoverExited { get; }

	HoverEnterEvent hoverEntered { get; }

	HoverExitEvent hoverExited { get; }

	List<IXRHoverInteractor> interactorsHovering { get; }

	bool isHovered { get; }

	bool IsHoverableBy(IXRHoverInteractor interactor);

	void OnHoverEntering(HoverEnterEventArgs args);

	void OnHoverEntered(HoverEnterEventArgs args);

	void OnHoverExiting(HoverExitEventArgs args);

	void OnHoverExited(HoverExitEventArgs args);
}
