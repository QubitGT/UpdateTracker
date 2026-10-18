using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Fusion.LagCompensation;

[Serializable]
public abstract class Query : IBoundsTraversalTest
{
	public QueryTriggerInteraction TriggerInteraction;

	public HitOptions Options;

	public LayerMask LayerMask;

	[NonSerialized]
	public PlayerRef Player;

	[NonSerialized]
	public int? Tick;

	[NonSerialized]
	public unsafe void* UserArgs;

	[NonSerialized]
	public float? Alpha;

	[NonSerialized]
	public int? TickTo;

	[NonSerialized]
	public PreProcessingDelegate PreProcessingDelegate;

	protected unsafe Query(ref QueryParams qParams)
	{
		PreProcessingDelegate = qParams.PreProcessingDelegate;
		TriggerInteraction = qParams.TriggerInteraction;
		LayerMask = qParams.LayerMask;
		UserArgs = qParams.UserArgs;
		Options = qParams.Options;
		TickTo = qParams.TickTo;
		Player = qParams.Player;
		Alpha = qParams.Alpha;
		Tick = qParams.Tick;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	bool IBoundsTraversalTest.Check(ref AABB bounds)
	{
		return Check(ref bounds);
	}

	protected abstract bool Check(ref AABB bounds);

	internal abstract bool NarrowPhase(IHitboxColliderContainer container, HashSet<int> candidates, List<HitboxHit> hits);

	internal abstract void PerformStaticQuery(NetworkRunner runner, List<LagCompensatedHit> hits, HitOptions options);

	internal HitboxHit CreateHitboxHit(ref HitboxCollider collider, Vector3 point, float distance, Vector3 normal)
	{
		HitboxHit result = default(HitboxHit);
		result.Point = point;
		result.Distance = distance;
		result.Normal = normal;
		result.Hitbox = collider.Hitbox;
		result.DebugTick = collider.DebugTick;
		result.DebugPosition = collider.Position;
		result.DebugRotation = collider.Rotation;
		result.Alpha = 0f;
		return result;
	}
}
