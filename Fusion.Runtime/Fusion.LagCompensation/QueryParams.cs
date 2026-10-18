using System;
using UnityEngine;

namespace Fusion.LagCompensation;

[Serializable]
public struct QueryParams
{
	public HitOptions Options;

	public QueryTriggerInteraction TriggerInteraction;

	public LayerMask LayerMask;

	[NonSerialized]
	public PlayerRef Player;

	[NonSerialized]
	public int Tick;

	[NonSerialized]
	public int? TickTo;

	[NonSerialized]
	public float? Alpha;

	[NonSerialized]
	public PreProcessingDelegate PreProcessingDelegate;

	[NonSerialized]
	public unsafe void* UserArgs;
}
