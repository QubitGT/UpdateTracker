using System;

namespace UnityEngine.Splines;

[Flags]
public enum EmbeddedSplineDataField
{
	Container = 1,
	SplineIndex = 2,
	Key = 4,
	Type = 8,
	All = 0xFF
}
