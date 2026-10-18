using System;
using UnityEngine.Serialization;

namespace Fusion;

[Serializable]
public class LagCompensationSettings
{
	[InlineHelp]
	public bool Enabled = false;

	[InlineHelp]
	[Unit(Units.MilliSecs)]
	[WarnIf("HitboxBufferLengthInMs", 300L, "Recommended value exceeded, unless a very high tick rate (100+) is intended.", CompareOperator.Greater)]
	[ErrorIf("HitboxBufferLengthInMs", 600L, "Recommended value exceeded, unless a very high tick rate (100+) is intended.", CompareOperator.Greater)]
	[RangeEx(30.0, 800.0, ClampMax = false)]
	public int HitboxBufferLengthInMs = 200;

	[FormerlySerializedAs("HitboxCapacity")]
	[InlineHelp]
	[Unit(Units.Count)]
	[RangeEx(16.0, 1024.0, ClampMax = false, UseSlider = false)]
	public int HitboxDefaultCapacity = 512;

	[Unit(Units.Count)]
	[InlineHelp]
	public int CachedStaticCollidersSize = 64;

	public float ExpansionFactor => 0.2f;

	public bool Optimize => false;
}
