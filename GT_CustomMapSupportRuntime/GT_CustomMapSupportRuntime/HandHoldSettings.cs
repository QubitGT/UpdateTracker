using UnityEngine;

namespace GT_CustomMapSupportRuntime;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class HandHoldSettings : MonoBehaviour
{
	public enum HandSnapMethod
	{
		None,
		SnapToCenterPoint,
		SnapToNearestEdge,
		SnapToXAxisPoint,
		SnapToYAxisPoint,
		SnapToZAxisPoint
	}

	public HandSnapMethod handSnapMethod;

	public bool rotatePlayerWhenHeld;

	[Tooltip("If TRUE, players will be able to perform the Grab action before their hand collides with this HandHold and it will still be grabbed once their hand comes in contact with the HandHold. If FALSE, players must perform the Grab action while their hand is already near the HandHold for it to be grabbed.")]
	public bool allowPreGrab;
}
