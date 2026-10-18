using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit;

public class InteractionGroupUnregisteredEventArgs : BaseRegistrationEventArgs
{
	public IXRInteractionGroup interactionGroupObject { get; set; }
}
