using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Interactables;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public static class XRSelectInteractableExtensions
{
	public static IXRSelectInteractor GetOldestInteractorSelecting(this IXRSelectInteractable interactable)
	{
		if (interactable == null || interactable.interactorsSelecting.Count <= 0)
		{
			return null;
		}
		return interactable.interactorsSelecting[0];
	}

	public static bool IsSelectedByLeft(this IXRSelectInteractable interactable)
	{
		return IsSelectedBy(interactable, InteractorHandedness.Left);
	}

	public static bool IsSelectedByRight(this IXRSelectInteractable interactable)
	{
		return IsSelectedBy(interactable, InteractorHandedness.Right);
	}

	private static bool IsSelectedBy(IXRSelectInteractable interactable, InteractorHandedness handedness)
	{
		List<IXRSelectInteractor> interactorsSelecting = interactable.interactorsSelecting;
		for (int i = 0; i < interactorsSelecting.Count; i++)
		{
			if (interactorsSelecting[i].handedness == handedness)
			{
				return true;
			}
		}
		return false;
	}
}
