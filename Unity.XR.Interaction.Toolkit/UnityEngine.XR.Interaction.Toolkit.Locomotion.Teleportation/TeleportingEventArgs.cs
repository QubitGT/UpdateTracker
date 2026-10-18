using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public class TeleportingEventArgs : BaseInteractionEventArgs
{
	public TeleportRequest teleportRequest { get; set; }
}
