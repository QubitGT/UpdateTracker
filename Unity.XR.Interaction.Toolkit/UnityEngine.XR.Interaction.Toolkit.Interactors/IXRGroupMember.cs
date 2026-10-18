using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRGroupMember
{
	IXRInteractionGroup containingGroup { get; }

	void OnRegisteringAsGroupMember(IXRInteractionGroup group);

	void OnRegisteringAsNonGroupMember();
}
