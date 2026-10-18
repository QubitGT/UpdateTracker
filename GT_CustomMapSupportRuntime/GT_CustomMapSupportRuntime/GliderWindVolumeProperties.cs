using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public struct GliderWindVolumeProperties
{
	public float maxSpeed;

	public float maxAccel;

	public AnimationCurve speedVsAccelCurve;

	public Vector3 localWindDirection;
}
