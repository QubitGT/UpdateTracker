using UnityEngine;

namespace Meta.XR;

public struct EnvironmentRaycastHit
{
	public EnvironmentRaycastHitStatus status;

	public Vector3 point;

	public Vector3 normal;

	public float normalConfidence;
}
