#define DEBUG
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.LagCompensation;

[Serializable]
public class BoxOverlapQuery : Query
{
	public Vector3 Center;

	public Vector3 Extents;

	public Quaternion Rotation;

	private Collider[] _physXOverlapHits;

	private Collider2D[] _box2DOverlapHits;

	internal LagCompensationUtils.BoxNarrowData _queryNarrowData;

	public BoxOverlapQuery(ref BoxOverlapQueryParams boxOverlapParams)
		: base(ref boxOverlapParams.QueryParams)
	{
		Rotation = boxOverlapParams.Rotation;
		Extents = boxOverlapParams.Extents;
		Center = boxOverlapParams.Center;
		_physXOverlapHits = new Collider[boxOverlapParams.StaticHitsCapacity];
		_box2DOverlapHits = new Collider2D[boxOverlapParams.StaticHitsCapacity];
		_queryNarrowData = new LagCompensationUtils.BoxNarrowData(Center, Rotation, Extents);
	}

	public BoxOverlapQuery(ref BoxOverlapQueryParams boxOverlapParams, Collider[] physXOverlapHitsCache, Collider2D[] box2DOverlapHitsCache)
		: base(ref boxOverlapParams.QueryParams)
	{
		Rotation = boxOverlapParams.Rotation;
		Extents = boxOverlapParams.Extents;
		Center = boxOverlapParams.Center;
		_physXOverlapHits = physXOverlapHitsCache;
		_box2DOverlapHits = box2DOverlapHitsCache;
		_queryNarrowData = new LagCompensationUtils.BoxNarrowData(Center, Rotation, Extents);
	}

	protected override bool Check(ref AABB bounds)
	{
		Vector3 vector = Rotation * Extents;
		Vector3 pointB = Center + vector;
		Vector3 pointA = Center - vector;
		AABB aABB = new AABB(Center, pointA, pointB);
		return aABB.Min.x <= bounds.Max.x && aABB.Max.x >= bounds.Min.x && aABB.Min.y <= bounds.Max.y && aABB.Max.y >= bounds.Min.y && aABB.Min.z <= bounds.Max.z && aABB.Max.z >= bounds.Min.z;
	}

	internal override bool NarrowPhase(IHitboxColliderContainer container, HashSet<int> candidates, List<HitboxHit> hits)
	{
		_queryNarrowData = PreComputeNarrowData();
		bool result = false;
		foreach (int candidate in candidates)
		{
			ref HitboxCollider collider = ref container.GetCollider(candidate);
			if (NarrowPhaseBox(ref _queryNarrowData, ref collider, computeDetailedInfo: true, out var hitPoint, out var hitNormal))
			{
				result = true;
				hits.Add(CreateHitboxHit(ref collider, hitPoint, 0f, hitNormal));
			}
		}
		return result;
	}

	internal override void PerformStaticQuery(NetworkRunner runner, List<LagCompensatedHit> hits, HitOptions options)
	{
		if ((options & HitOptions.IncludePhysX) != 0)
		{
			int num = runner.GetPhysicsScene().OverlapBox(Center, Extents, _physXOverlapHits, Rotation, LayerMask, TriggerInteraction);
			for (int i = 0; i < num; i++)
			{
				Collider collider = _physXOverlapHits[i];
				LagCompensatedHit lagCompensatedHit = default(LagCompensatedHit);
				lagCompensatedHit.Collider = collider;
				lagCompensatedHit.Normal = default(Vector3);
				lagCompensatedHit.Distance = 0f;
				lagCompensatedHit.GameObject = collider.gameObject;
				lagCompensatedHit.Type = HitType.PhysX;
				LagCompensatedHit item = lagCompensatedHit;
				hits.Add(item);
			}
		}
		else if ((options & HitOptions.IncludeBox2D) != 0)
		{
			Rotation.ToAngleAxis(out var angle, out var _);
			int num2 = runner.GetPhysicsScene2D().OverlapBox(Center, Extents, angle, _box2DOverlapHits, LayerMask);
			for (int j = 0; j < num2; j++)
			{
				Collider2D collider2D = _box2DOverlapHits[j];
				LagCompensatedHit lagCompensatedHit = default(LagCompensatedHit);
				lagCompensatedHit.Collider2D = collider2D;
				lagCompensatedHit.Normal = default(Vector3);
				lagCompensatedHit.Distance = 0f;
				lagCompensatedHit.GameObject = collider2D.gameObject;
				lagCompensatedHit.Type = HitType.Box2D;
				LagCompensatedHit item2 = lagCompensatedHit;
				hits.Add(item2);
			}
		}
	}

	internal bool NarrowPhaseBox(ref LagCompensationUtils.BoxNarrowData boxQueryNarrowData, ref HitboxCollider c, bool computeDetailedInfo, out Vector3 hitPoint, out Vector3 hitNormal)
	{
		LagCompensationUtils.ContactData contact;
		switch (c.Type)
		{
		case HitboxTypes.Box:
			Assert.Check(c.IsBoxNarrowDataInitialized);
			return LagCompensationUtils.NarrowBoxBox(ref boxQueryNarrowData, ref c.BoxNarrowData, computeDetailedInfo, out hitPoint, out hitNormal);
		case HitboxTypes.Sphere:
		{
			Vector3 sphereCenter = boxQueryNarrowData.WorldToLocalPoint(c.Position + c.Rotation * c.Offset);
			if (LagCompensationUtils.LocalAABBSphereContact(boxQueryNarrowData.Extents, sphereCenter, c.Radius, out contact))
			{
				hitPoint = boxQueryNarrowData.LocalToWorldPoint(contact.Point);
				hitNormal = boxQueryNarrowData.LocalToWorldVector(-contact.Normal);
				return true;
			}
			break;
		}
		case HitboxTypes.Capsule:
		{
			Vector3 localCapsulePointA = boxQueryNarrowData.WorldToLocalPoint(c.LocalToWorld.MultiplyPoint3x4(c.CapsuleLocalTopCenter));
			Vector3 localCapsulePointB = boxQueryNarrowData.WorldToLocalPoint(c.LocalToWorld.MultiplyPoint3x4(c.CapsuleLocalBottomCenter));
			Vector3 localCapsuleCenter = boxQueryNarrowData.WorldToLocalPoint(c.Position + c.Offset);
			if (LagCompensationUtils.LocalAABBCapsuleIntersection(localCapsuleCenter, localCapsulePointA, localCapsulePointB, c.Radius, boxQueryNarrowData.Extents, out contact))
			{
				hitPoint = boxQueryNarrowData.LocalToWorldPoint(contact.Point);
				hitNormal = boxQueryNarrowData.LocalToWorldVector(contact.Normal);
				return true;
			}
			break;
		}
		}
		hitPoint = default(Vector3);
		hitNormal = default(Vector3);
		return false;
	}

	internal LagCompensationUtils.BoxNarrowData PreComputeNarrowData()
	{
		bool flag;
		LagCompensationUtils.BoxNarrowData result;
		if (Rotation == Quaternion.identity)
		{
			flag = false;
			result = default(LagCompensationUtils.BoxNarrowData);
		}
		else
		{
			flag = true;
			result = new LagCompensationUtils.BoxNarrowData(Center, Rotation, Extents);
			Vector3 start = result.BoxEdgesRotated.E00.Start;
			Vector3 start2 = result.BoxEdgesRotated.E01.Start;
			Vector3 start3 = result.BoxEdgesRotated.E02.Start;
			Vector3 start4 = result.BoxEdgesRotated.E03.Start;
			start.x = Mathf.Abs(start.x);
			start.y = Mathf.Abs(start.y);
			start.z = Mathf.Abs(start.z);
			start2.x = Mathf.Abs(start2.x);
			start2.y = Mathf.Abs(start2.y);
			start2.z = Mathf.Abs(start2.z);
			start3.x = Mathf.Abs(start3.x);
			start3.y = Mathf.Abs(start3.y);
			start3.z = Mathf.Abs(start3.z);
			start4.x = Mathf.Abs(start4.x);
			start4.y = Mathf.Abs(start4.y);
			start4.z = Mathf.Abs(start4.z);
			Vector3 vector = default(Vector3);
			vector.x = Mathf.Max(start.x, start2.x);
			vector.y = Mathf.Max(start.y, start2.y);
			vector.z = Mathf.Max(start.z, start2.z);
			vector.x = Mathf.Max(vector.x, start3.x);
			vector.y = Mathf.Max(vector.y, start3.y);
			vector.z = Mathf.Max(vector.z, start3.z);
			vector.x = Mathf.Max(vector.x, start4.x);
			vector.y = Mathf.Max(vector.y, start4.y);
			vector.z = Mathf.Max(vector.z, start4.z);
		}
		if (!flag)
		{
			result = new LagCompensationUtils.BoxNarrowData(Center, Rotation, Extents);
		}
		return result;
	}
}
