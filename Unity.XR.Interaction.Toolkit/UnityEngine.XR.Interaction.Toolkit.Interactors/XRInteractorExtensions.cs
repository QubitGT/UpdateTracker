using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public static class XRInteractorExtensions
{
	public static bool IsBlockedByInteractionWithinGroup(this IXRInteractor interactor)
	{
		if (!(interactor is IXRGroupMember groupMember))
		{
			return false;
		}
		IXRInteractionGroup topLevelContainingGroup = groupMember.GetTopLevelContainingGroup();
		if (topLevelContainingGroup == null)
		{
			return false;
		}
		IXRInteractor activeInteractor = topLevelContainingGroup.activeInteractor;
		if (activeInteractor != null)
		{
			return activeInteractor != interactor;
		}
		return false;
	}
}
