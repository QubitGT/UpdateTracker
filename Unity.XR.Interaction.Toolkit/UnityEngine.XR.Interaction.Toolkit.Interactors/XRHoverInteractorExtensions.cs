using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public static class XRHoverInteractorExtensions
{
	public static IXRHoverInteractable GetOldestInteractableHovered(this IXRHoverInteractor interactor)
	{
		if (interactor == null || interactor.interactablesHovered.Count <= 0)
		{
			return null;
		}
		return interactor.interactablesHovered[0];
	}
}
