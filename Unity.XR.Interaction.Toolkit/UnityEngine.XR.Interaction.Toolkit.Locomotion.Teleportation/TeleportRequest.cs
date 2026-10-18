using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public struct TeleportRequest
{
	public Vector3 destinationPosition;

	public Quaternion destinationRotation;

	public float requestTime;

	public MatchOrientation matchOrientation;
}
