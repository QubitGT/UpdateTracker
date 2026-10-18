using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Interactables;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public static class XRHoverInteractableExtensions
{
	public static IXRHoverInteractor GetOldestInteractorHovering(this IXRHoverInteractable interactable)
	{
		if (interactable == null || interactable.interactorsHovering.Count <= 0)
		{
			return null;
		}
		return interactable.interactorsHovering[0];
	}

	public static bool IsHoveredByLeft(this IXRHoverInteractable interactable)
	{
		return IsHoveredBy(interactable, InteractorHandedness.Left);
	}

	public static bool IsHoveredByRight(this IXRHoverInteractable interactable)
	{
		return IsHoveredBy(interactable, InteractorHandedness.Right);
	}

	private static bool IsHoveredBy(IXRHoverInteractable interactable, InteractorHandedness handedness)
	{
		List<IXRHoverInteractor> interactorsHovering = interactable.interactorsHovering;
		for (int i = 0; i < interactorsHovering.Count; i++)
		{
			if (interactorsHovering[i].handedness == handedness)
			{
				return true;
			}
		}
		return false;
	}
}
