using UnityEngine;

namespace Unity.XR.CoreUtils;

public static class BoundsExtensions
{
	public static bool ContainsCompletely(this Bounds outerBounds, Bounds innerBounds)
	{
		Vector3 max = outerBounds.max;
		Vector3 min = outerBounds.min;
		Vector3 max2 = innerBounds.max;
		Vector3 min2 = innerBounds.min;
		if (max.x >= max2.x && max.y >= max2.y && max.z >= max2.z && min.x <= min2.x && min.y <= min2.y)
		{
			return min.z <= min2.z;
		}
		return false;
	}
}
