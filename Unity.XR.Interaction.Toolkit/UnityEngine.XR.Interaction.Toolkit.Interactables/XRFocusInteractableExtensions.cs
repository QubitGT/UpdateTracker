using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Interactables;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public static class XRFocusInteractableExtensions
{
	public static IXRInteractionGroup GetOldestInteractorFocusing(this IXRFocusInteractable interactable)
	{
		if (interactable == null || interactable.interactionGroupsFocusing.Count <= 0)
		{
			return null;
		}
		return interactable.interactionGroupsFocusing[0];
	}
}
