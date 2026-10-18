using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class ColliderMask : Mask
{
	[SerializeField]
	private int MaxCheckColliders = 10;

	[SerializeField]
	private bool IgnoreFloorCollision = true;

	[SerializeField]
	private bool IgnoreGlobalMeshCollision = true;

	[SerializeField]
	private LayerMask CheckLayers = -1;

	public override float SampleMask(Candidate c)
	{
		return 0f;
	}

	public override bool Check(Candidate c)
	{
		Collider componentInChildren = c.decorationPrefab.GetComponentInChildren<Collider>();
		if (componentInChildren == null)
		{
			return true;
		}
		Collider[] array = new Collider[MaxCheckColliders];
		if (componentInChildren is BoxCollider boxCollider)
		{
			int size = Physics.OverlapBoxNonAlloc(c.hit.point, boxCollider.size / 2f, array, Quaternion.identity, CheckLayers);
			return CheckColliderHitsForMRUK(array, size);
		}
		if (componentInChildren is MeshCollider meshCollider)
		{
			BoxCollider boxCollider2 = c.decorationPrefab.AddComponent<BoxCollider>();
			boxCollider2.center = meshCollider.bounds.center - c.decorationPrefab.transform.position;
			boxCollider2.size = meshCollider.bounds.size;
			int size2 = Physics.OverlapBoxNonAlloc(c.hit.point, boxCollider2.size / 2f, array, Quaternion.identity, CheckLayers);
			Object.Destroy(boxCollider2);
			return CheckColliderHitsForMRUK(array, size2);
		}
		if (componentInChildren is CapsuleCollider capsuleCollider)
		{
			int size3 = Physics.OverlapCapsuleNonAlloc(capsuleCollider.transform.position, c.hit.point + Vector3.up * capsuleCollider.height, capsuleCollider.radius, array, CheckLayers);
			return CheckColliderHitsForMRUK(array, size3);
		}
		if (componentInChildren is SphereCollider sphereCollider)
		{
			int size4 = Physics.OverlapSphereNonAlloc(c.hit.point, sphereCollider.radius, array, CheckLayers);
			return CheckColliderHitsForMRUK(array, size4);
		}
		return false;
	}

	private bool CheckColliderHitsForMRUK(Collider[] colliders, int size)
	{
		bool result = true;
		for (int i = 0; i < MaxCheckColliders; i++)
		{
			if (!(colliders[i] == null))
			{
				if (colliders[i].gameObject.GetComponentsInParent<MRUKAnchor>().Length == 0)
				{
					result = false;
					break;
				}
				MRUKAnchor obj = colliders[i].gameObject.GetComponentsInParent<MRUKAnchor>()[0];
				if (obj.Label == MRUKAnchor.SceneLabels.FLOOR)
				{
					result = IgnoreFloorCollision;
				}
				if (obj.Label == MRUKAnchor.SceneLabels.GLOBAL_MESH)
				{
					result = IgnoreGlobalMeshCollision;
				}
			}
		}
		return result;
	}
}
