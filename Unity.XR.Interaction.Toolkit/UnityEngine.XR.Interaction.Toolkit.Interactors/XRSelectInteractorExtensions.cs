using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public static class XRSelectInteractorExtensions
{
	public static IXRSelectInteractable GetOldestInteractableSelected(this IXRSelectInteractor interactor)
	{
		if (interactor == null || interactor.interactablesSelected.Count <= 0)
		{
			return null;
		}
		return interactor.interactablesSelected[0];
	}
}
