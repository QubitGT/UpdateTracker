using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.LagCompensation;

[Serializable]
public class RaycastAllQuery : RaycastQuery
{
	private RaycastHit[] _physXRaycastHits;

	private RaycastHit2D[] _box2DRaycastHits;

	public RaycastAllQuery(ref RaycastQueryParams raycastQueryParams)
		: base(ref raycastQueryParams)
	{
		_physXRaycastHits = new RaycastHit[raycastQueryParams.StaticHitsCapacity];
		_box2DRaycastHits = new RaycastHit2D[raycastQueryParams.StaticHitsCapacity];
	}

	public RaycastAllQuery(ref RaycastQueryParams raycastQueryParams, RaycastHit[] physXRaycastHitsCache, RaycastHit2D[] box2DRaycastHitCache)
		: base(ref raycastQueryParams)
	{
		_physXRaycastHits = physXRaycastHitsCache;
		_box2DRaycastHits = box2DRaycastHitCache;
	}

	internal override bool NarrowPhase(IHitboxColliderContainer container, HashSet<int> candidates, List<HitboxHit> hits)
	{
		int count = hits.Count;
		foreach (int candidate in candidates)
		{
			ref HitboxCollider collider = ref container.GetCollider(candidate);
			if (NarrowPhaseRay(ref collider, Origin, Direction, Length, out var point, out var normal, out var distance))
			{
				hits.Add(CreateHitboxHit(ref collider, point, distance, normal));
			}
		}
		return hits.Count > count;
	}

	internal override void PerformStaticQuery(NetworkRunner runner, List<LagCompensatedHit> hits, HitOptions options)
	{
		if ((options & HitOptions.IncludePhysX) != 0)
		{
			int num = runner.GetPhysicsScene().Raycast(Origin, Direction, _physXRaycastHits, Length, LayerMask, TriggerInteraction);
			for (int i = 0; i < num; i++)
			{
				hits.Add((LagCompensatedHit)_physXRaycastHits[i]);
			}
		}
		else if ((options & HitOptions.IncludeBox2D) != 0)
		{
			int num2 = runner.GetPhysicsScene2D().Raycast(Origin, Direction, Length, _box2DRaycastHits, LayerMask);
			for (int j = 0; j < num2; j++)
			{
				hits.Add((LagCompensatedHit)_box2DRaycastHits[j]);
			}
		}
	}
}
