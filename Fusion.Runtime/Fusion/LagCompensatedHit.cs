using System.Collections.Generic;
using Fusion.LagCompensation;
using UnityEngine;

namespace Fusion;

public struct LagCompensatedHit
{
	public HitType Type;

	public GameObject GameObject;

	public Vector3 Normal;

	public Vector3 Point;

	public Vector3 HitboxColliderPosition;

	public Quaternion HitboxColliderRotation;

	public float Distance;

	public Hitbox Hitbox;

	public Collider Collider;

	public Collider2D Collider2D;

	internal float _sortAux;

	public static explicit operator LagCompensatedHit(RaycastHit raycastHit)
	{
		LagCompensatedHit result = default(LagCompensatedHit);
		result.Normal = raycastHit.normal;
		result.Distance = raycastHit.distance;
		result.Point = raycastHit.point;
		result.HitboxColliderPosition = default(Vector3);
		result.HitboxColliderRotation = default(Quaternion);
		result.GameObject = raycastHit.collider.gameObject;
		result.Hitbox = null;
		result.Collider = raycastHit.collider;
		result.Type = HitType.PhysX;
		return result;
	}

	public static explicit operator LagCompensatedHit(RaycastHit2D raycastHit2D)
	{
		LagCompensatedHit result = default(LagCompensatedHit);
		result.Normal = raycastHit2D.normal;
		result.Distance = raycastHit2D.distance;
		result.Point = raycastHit2D.point;
		result.HitboxColliderPosition = default(Vector3);
		result.HitboxColliderRotation = default(Quaternion);
		result.GameObject = raycastHit2D.collider.gameObject;
		result.Hitbox = null;
		result.Collider2D = raycastHit2D.collider;
		result.Type = HitType.Box2D;
		return result;
	}

	internal static LagCompensatedHit FromHitboxHit(ref HitboxHit hitboxHit)
	{
		LagCompensatedHit result = default(LagCompensatedHit);
		result.Normal = hitboxHit.Normal;
		result.Distance = hitboxHit.Distance;
		result.Point = hitboxHit.Point;
		result.HitboxColliderPosition = hitboxHit.DebugPosition;
		result.HitboxColliderRotation = hitboxHit.DebugRotation;
		result.GameObject = hitboxHit.Hitbox.gameObject;
		result.Hitbox = hitboxHit.Hitbox;
		result.Collider = null;
		result.Type = HitType.Hitbox;
		return result;
	}

	internal static void QuickSort(List<LagCompensatedHit> hits, int low, int high)
	{
		if (low >= high)
		{
			return;
		}
		float sortAux = hits[high]._sortAux;
		int num = low;
		LagCompensatedHit value;
		for (int i = low; i < high; i++)
		{
			if (hits[i]._sortAux < sortAux)
			{
				value = hits[num];
				hits[num] = hits[i];
				hits[i] = value;
				num++;
			}
		}
		value = hits[num];
		hits[num] = hits[high];
		hits[high] = value;
		QuickSort(hits, low, num - 1);
		QuickSort(hits, num + 1, high);
	}

	internal static void QuickSortDistance(List<LagCompensatedHit> hits, int low, int high)
	{
		if (low >= high)
		{
			return;
		}
		float distance = hits[high].Distance;
		int num = low;
		LagCompensatedHit value;
		for (int i = low; i < high; i++)
		{
			if (hits[i].Distance < distance)
			{
				value = hits[num];
				hits[num] = hits[i];
				hits[i] = value;
				num++;
			}
		}
		value = hits[num];
		hits[num] = hits[high];
		hits[high] = value;
		QuickSortDistance(hits, low, num - 1);
		QuickSortDistance(hits, num + 1, high);
	}
}
