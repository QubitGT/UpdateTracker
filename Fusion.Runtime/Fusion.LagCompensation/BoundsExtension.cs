using System.Runtime.CompilerServices;
using UnityEngine;

namespace Fusion.LagCompensation;

internal static class BoundsExtension
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool ContainBounds(this Bounds bounds, Bounds target)
	{
		return bounds.Contains(target.min) && bounds.Contains(target.max);
	}
}
