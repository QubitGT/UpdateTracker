using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Interactables;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public struct DistanceInfo
{
	public Vector3 point { get; set; }

	public float distanceSqr { get; set; }

	public Collider collider { get; set; }
}
