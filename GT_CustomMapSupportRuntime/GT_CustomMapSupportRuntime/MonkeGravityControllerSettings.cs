using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public class MonkeGravityControllerSettings : MonoBehaviour
{
	public enum RotationDirection
	{
		None,
		Forward,
		Backward,
		Left,
		Right
	}

	[Tooltip("Collider used by gravity zones to detect this object. Optional when Always In Zone is set; otherwise falls back to a collider on this GameObject.")]
	public Collider? activatorCollider;

	[Tooltip("Rigidbody the gravity forces are applied to. Falls back to a rigidbody on this GameObject.")]
	public Rigidbody? targetRigidbody;

	[Tooltip("Transform used for position/rotation. Falls back to this GameObject's transform.")]
	public Transform? targetTransform;

	[Header("Rotation Settings")]
	[Tooltip("If enabled, the target snaps to the gravity up direction instead of rotating over time.")]
	public bool instantRotation;

	[Tooltip("If enabled, the target rotates to align with the gravity up direction.")]
	public bool useRotation;

	[Tooltip("The direction we wish to rotate if the target is 180 degrees off.")]
	public RotationDirection preferredRotationDirection = RotationDirection.Right;

	[Header("Force Settings")]
	[Tooltip("If enabled, gravity forces are applied with the Force Mode Override instead of the mode requested by the zone.")]
	public bool overrideForceMode;

	public ForceMode forceModeOverride;

	[Tooltip("Optional gravity zone this controller is permanently inside, without needing trigger contact. When set, the Activator Collider is not required.")]
	public BasicGravityZoneSettings? alwaysInZone;
}
