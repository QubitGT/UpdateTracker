using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.LagCompensation;

[Serializable]
public class SphereOverlapQuery : Query
{
	public Vector3 Center;

	public float Radius;

	private Collider[] _physXOverlapHits;

	private Collider2D[] _box2DOverlapHits;

	public SphereOverlapQuery(ref SphereOverlapQueryParams sphereOverlapParams)
		: base(ref sphereOverlapParams.QueryParams)
	{
		Center = sphereOverlapParams.Center;
		Radius = sphereOverlapParams.Radius;
		_physXOverlapHits = new Collider[sphereOverlapParams.StaticHitsCapacity];
		_box2DOverlapHits = new Collider2D[sphereOverlapParams.StaticHitsCapacity];
	}

	public SphereOverlapQuery(ref SphereOverlapQueryParams sphereOverlapParams, Collider[] physXOverlapHitsCache, Collider2D[] box2DOverlapHitsCache)
		: base(ref sphereOverlapParams.QueryParams)
	{
		Center = sphereOverlapParams.Center;
		Radius = sphereOverlapParams.Radius;
		_physXOverlapHits = physXOverlapHitsCache;
		_box2DOverlapHits = box2DOverlapHitsCache;
	}

	protected override bool Check(ref AABB bounds)
	{
		return LagCompensationUtils.LocalAABBSphereIntersection(bounds.Extents, Center - bounds.Center, Radius);
	}

	internal override bool NarrowPhase(IHitboxColliderContainer container, HashSet<int> candidates, List<HitboxHit> hits)
	{
		bool result = false;
		foreach (int candidate in candidates)
		{
			ref HitboxCollider collider = ref container.GetCollider(candidate);
			if (NarrowPhaseSphere(ref collider, Center, Radius, out var point, out var normal))
			{
				result = true;
				hits.Add(CreateHitboxHit(ref collider, point, 0f, normal));
			}
		}
		return result;
	}

	internal override void PerformStaticQuery(NetworkRunner runner, List<LagCompensatedHit> hits, HitOptions options)
	{
		if ((options & HitOptions.IncludePhysX) != 0)
		{
			int num = runner.GetPhysicsScene().OverlapSphere(Center, Radius, _physXOverlapHits, LayerMask, TriggerInteraction);
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
			int num2 = runner.GetPhysicsScene2D().OverlapCircle(Center, Radius, _box2DOverlapHits, LayerMask);
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

	internal bool NarrowPhaseSphere(ref HitboxCollider c, Vector3 origin, float radius, out Vector3 point, out Vector3 normal)
	{
		switch (c.Type)
		{
		case HitboxTypes.Box:
		{
			Vector3 sphereCenter2 = c.LocalToWorld.inverse.MultiplyPoint(origin) - c.Offset;
			if (LagCompensationUtils.LocalAABBSphereContact(c.BoxExtents, sphereCenter2, radius, out var contact))
			{
				point = c.LocalToWorld.MultiplyPoint(contact.Point + c.Offset);
				normal = c.LocalToWorld.MultiplyVector(contact.Normal);
				return true;
			}
			break;
		}
		case HitboxTypes.Sphere:
		{
			Vector3 centerB = c.LocalToWorld.MultiplyPoint(c.Offset);
			if (LagCompensationUtils.SphereSphere(origin, radius, centerB, c.Radius, out point, out normal))
			{
				return true;
			}
			break;
		}
		case HitboxTypes.Capsule:
		{
			Vector3 sphereCenter = c.LocalToWorld.inverse.MultiplyPoint(origin);
			if (LagCompensationUtils.LocalSphereCapsuleIntersection(c.CapsuleLocalTopCenter, c.CapsuleLocalBottomCenter, c.Radius, sphereCenter, radius, out var contactData))
			{
				point = c.LocalToWorld.MultiplyPoint(contactData.Point);
				normal = c.LocalToWorld.MultiplyVector(contactData.Normal);
				return true;
			}
			break;
		}
		}
		point = default(Vector3);
		normal = default(Vector3);
		return false;
	}
}
