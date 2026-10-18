using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

public struct Candidate
{
	public GameObject decorationPrefab;

	public Vector2 localPos;

	public Vector2 localPosNormalized;

	public RaycastHit hit;

	public Vector3 anchorCompDists;

	public float anchorDist;

	public float slope;
}
