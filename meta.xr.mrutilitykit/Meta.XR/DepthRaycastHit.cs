using UnityEngine;

namespace Meta.XR;

internal struct DepthRaycastHit
{
	public DepthRaycastResult result;

	public Vector3 point;

	public Vector3 normal;

	public float normalConfidence;
}
