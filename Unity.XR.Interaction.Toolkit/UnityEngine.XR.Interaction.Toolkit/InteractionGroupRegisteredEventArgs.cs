using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit;

public class InteractionGroupRegisteredEventArgs : BaseRegistrationEventArgs
{
	public IXRInteractionGroup interactionGroupObject { get; set; }

	public IXRInteractionGroup containingGroupObject { get; set; }
}
