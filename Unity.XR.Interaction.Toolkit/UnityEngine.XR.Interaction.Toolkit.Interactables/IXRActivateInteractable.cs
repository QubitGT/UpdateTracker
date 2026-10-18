using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Interactables;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRActivateInteractable : IXRInteractable
{
	ActivateEvent activated { get; }

	DeactivateEvent deactivated { get; }

	void OnActivated(ActivateEventArgs args);

	void OnDeactivated(DeactivateEventArgs args);
}
