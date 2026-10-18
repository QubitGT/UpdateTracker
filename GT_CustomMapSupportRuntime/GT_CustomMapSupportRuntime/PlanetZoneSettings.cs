using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public class PlanetZoneSettings : BasicGravityZoneSettings
{
	[Tooltip("how close to the center of the zone to enable rotating the player")]
	public float rotationDistance;

	[Tooltip("if enabled, always rotates the player")]
	public bool alwaysRotate = true;

	[Tooltip("if enabled, gravity strength is read from the curve below using distance from the zone's center, instead of the constant gravityStrength")]
	public bool useGravityCurve;

	[Tooltip("Maps distance from the zone's center (x) to gravity strength (y). Negative y pulls toward center, positive y expels.")]
	[SerializeField]
	public AnimationCurve gravityCurve = AnimationCurve.Constant(0f, 1f, -9.81f);
}
