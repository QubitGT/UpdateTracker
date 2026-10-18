using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public static class XRGroupMemberExtensions
{
	public static IXRInteractionGroup GetTopLevelContainingGroup(this IXRGroupMember groupMember)
	{
		IXRInteractionGroup iXRInteractionGroup = groupMember.containingGroup;
		for (IXRInteractionGroup iXRInteractionGroup2 = iXRInteractionGroup; iXRInteractionGroup2 != null; iXRInteractionGroup2 = ((iXRInteractionGroup is IXRGroupMember iXRGroupMember) ? iXRGroupMember.containingGroup : null))
		{
			iXRInteractionGroup = iXRInteractionGroup2;
		}
		return iXRInteractionGroup;
	}
}
