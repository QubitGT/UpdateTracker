using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public class TorusZoneSettings : BasicGravityZoneSettings
{
	[Tooltip("Major radius of the torus (distance from torus center to the centerline of the tube). Torus axis is transform.up.")]
	public float majorRadius = 5f;

	[Tooltip("how close to the central ring of the torus to enable rotating the player")]
	public float rotationDistance;

	[Tooltip("if enabled, always rotates the player")]
	public bool alwaysRotate = true;
}
