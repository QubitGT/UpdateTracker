using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.LagCompensation;

[Serializable]
public class RaycastQuery : Query
{
	public Vector3 Direction;

	public Vector3 Origin;

	public float Length;

	private RaycastHit _raycastHit;

	private RaycastHit2D _raycastHit2D;

	public RaycastQuery(ref RaycastQueryParams raycastQueryParams)
		: base(ref raycastQueryParams.QueryParams)
	{
		Direction = raycastQueryParams.Direction;
		Origin = raycastQueryParams.Origin;
		Length = raycastQueryParams.Length;
	}

	protected override bool Check(ref AABB bounds)
	{
		Vector3 min = bounds.Min;
		Vector3 max = bounds.Max;
		float num = Length * Length;
		bool flag = true;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		Vector3 vector = default(Vector3);
		Vector3 vector2 = default(Vector3);
		Vector3 vector3 = default(Vector3);
		if (Origin.x < min.x)
		{
			vector2.x = min.x;
			flag = false;
		}
		else if (Origin.x > max.x)
		{
			vector2.x = max.x;
			flag = false;
		}
		else
		{
			flag2 = true;
		}
		if (Origin.y < min.y)
		{
			vector2.y = min.y;
			flag = false;
		}
		else if (Origin.y > max.y)
		{
			vector2.y = max.y;
			flag = false;
		}
		else
		{
			flag3 = true;
		}
		if (Origin.z < min.z)
		{
			vector2.z = min.z;
			flag = false;
		}
		else if (Origin.z > max.z)
		{
			vector2.z = max.z;
			flag = false;
		}
		else
		{
			flag4 = true;
		}
		if (flag)
		{
			vector3 = Origin;
			return true;
		}
		if (Direction.x != 0f && !flag2)
		{
			vector.x = (vector2.x - Origin.x) / Direction.x;
		}
		else
		{
			vector.x = -1f;
		}
		if (Direction.y != 0f && !flag3)
		{
			vector.y = (vector2.y - Origin.y) / Direction.y;
		}
		else
		{
			vector.y = -1f;
		}
		if (Direction.z != 0f && !flag4)
		{
			vector.z = (vector2.z - Origin.z) / Direction.z;
		}
		else
		{
			vector.z = -1f;
		}
		int num2 = 0;
		float num3 = vector.x;
		if (num3 < vector.y)
		{
			num2 = 1;
			num3 = vector.y;
		}
		if (num3 < vector.z)
		{
			num2 = 2;
			num3 = vector.z;
		}
		if (num3 < 0f)
		{
			return false;
		}
		if (num2 != 0)
		{
			vector3.x = Origin.x + num3 * Direction.x;
			if (vector3.x < min.x || vector3.x > max.x)
			{
				return false;
			}
		}
		else
		{
			vector3.x = vector2.x;
		}
		if (num2 != 1)
		{
			vector3.y = Origin.y + num3 * Direction.y;
			if (vector3.y < min.y || vector3.y > max.y)
			{
				return false;
			}
		}
		else
		{
			vector3.y = vector2.y;
		}
		if (num2 != 2)
		{
			vector3.z = Origin.z + num3 * Direction.z;
			if (vector3.z < min.z || vector3.z > max.z)
			{
				return false;
			}
		}
		else
		{
			vector3.z = vector2.z;
		}
		Vector3 origin = Origin;
		origin.x -= vector3.x;
		origin.y -= vector3.y;
		origin.z -= vector3.z;
		if (origin.sqrMagnitude <= num)
		{
			return true;
		}
		return false;
	}

	internal override bool NarrowPhase(IHitboxColliderContainer container, HashSet<int> candidates, List<HitboxHit> hits)
	{
		int count = hits.Count;
		float num = float.MaxValue;
		foreach (int candidate in candidates)
		{
			ref HitboxCollider collider = ref container.GetCollider(candidate);
			if (NarrowPhaseRay(ref collider, Origin, Direction, Length, out var point, out var normal, out var distance) && !(distance >= num))
			{
				num = distance;
				hits.Insert(hits.Count, CreateHitboxHit(ref collider, point, distance, normal));
			}
		}
		return hits.Count > count;
	}

	internal override void PerformStaticQuery(NetworkRunner runner, List<LagCompensatedHit> hits, HitOptions options)
	{
		if ((options & HitOptions.IncludePhysX) != 0)
		{
			if (runner.GetPhysicsScene().Raycast(Origin, Direction, out _raycastHit, Length, LayerMask, TriggerInteraction))
			{
				hits.Add((LagCompensatedHit)_raycastHit);
			}
		}
		else if ((options & HitOptions.IncludeBox2D) != 0)
		{
			_raycastHit2D = runner.GetPhysicsScene2D().Raycast(Origin, Direction, Length, LayerMask);
			if (_raycastHit2D.collider != null)
			{
				hits.Add((LagCompensatedHit)_raycastHit2D);
			}
		}
	}

	internal bool NarrowPhaseRay(ref HitboxCollider c, Vector3 origin, Vector3 direction, float length, out Vector3 point, out Vector3 normal, out float distance)
	{
		switch (c.Type)
		{
		case HitboxTypes.Box:
		{
			Matrix4x4 inverse2 = c.LocalToWorld.inverse;
			Vector3 origin2 = inverse2.MultiplyPoint(origin) - c.Offset;
			Vector3 dir = inverse2.MultiplyVector(direction);
			Vector3 minB = -c.BoxExtents;
			Vector3 maxB = c.BoxExtents;
			if (LagCompensationUtils.RayAABB(ref minB, ref maxB, ref origin2, ref dir, length * length, out point, out normal, out distance))
			{
				point = c.LocalToWorld.MultiplyPoint(point + c.Offset);
				normal = c.LocalToWorld.MultiplyVector(normal);
				return true;
			}
			break;
		}
		case HitboxTypes.Sphere:
		{
			Vector3 center = c.LocalToWorld.MultiplyPoint(c.Offset);
			if (LagCompensationUtils.RaySphereIntersection(origin, direction, length, center, c.Radius, out point, out normal, out distance))
			{
				return true;
			}
			break;
		}
		case HitboxTypes.Capsule:
		{
			Matrix4x4 inverse = c.LocalToWorld.inverse;
			Vector3 rayLocalOrigin = inverse.MultiplyPoint(origin);
			Vector3 vector = inverse.MultiplyVector(direction);
			if (LagCompensationUtils.LocalRayCapsuleIntersection(c.CapsuleLocalTopCenter, c.CapsuleLocalBottomCenter, c.Radius, rayLocalOrigin, vector.normalized, length, out point, out normal, out distance))
			{
				point = c.LocalToWorld.MultiplyPoint(point);
				normal = c.LocalToWorld.MultiplyVector(normal);
				return true;
			}
			break;
		}
		}
		point = default(Vector3);
		normal = default(Vector3);
		distance = 0f;
		return false;
	}
}
