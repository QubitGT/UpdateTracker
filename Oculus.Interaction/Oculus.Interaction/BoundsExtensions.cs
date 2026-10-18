using UnityEngine;

namespace Oculus.Interaction;

public static class BoundsExtensions
{
	public static bool Clip(this Bounds bounds, in Bounds clipper, out Bounds result)
	{
		result = default(Bounds);
		Vector3 min = Vector3.Max(bounds.min, clipper.min);
		Vector3 max = Vector3.Min(bounds.max, clipper.max);
		if (min.x > max.x || min.y > max.y || min.z > max.z)
		{
			return false;
		}
		result.SetMinMax(min, max);
		return true;
	}
}
