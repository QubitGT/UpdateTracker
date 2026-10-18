using System;

namespace UnityEngine.Splines;

[Serializable]
public struct DistanceToInterpolation
{
	public float Distance;

	public float T;

	internal static readonly DistanceToInterpolation Invalid = new DistanceToInterpolation
	{
		Distance = -1f,
		T = -1f
	};
}
