using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface ITeleportationVolumeAnchorFilter
{
	int GetDestinationAnchorIndex(TeleportationMultiAnchorVolume teleportationVolume);
}
