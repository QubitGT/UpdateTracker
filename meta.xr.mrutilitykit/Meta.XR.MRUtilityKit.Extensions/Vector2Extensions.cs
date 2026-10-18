using UnityEngine;

namespace Meta.XR.MRUtilityKit.Extensions;

internal static class Vector2Extensions
{
	internal static Vector2 Floor(this Vector2 a)
	{
		return new Vector2(Mathf.Floor(a.x), Mathf.Floor(a.y));
	}

	internal static Vector2 Frac(this Vector2 a)
	{
		return new Vector2(a.x - Mathf.Floor(a.x), a.y - Mathf.Floor(a.y));
	}

	internal static Vector2 Add(this Vector2 a, float b)
	{
		return new Vector3(a.x + b, a.y + b);
	}

	internal static Vector2 Abs(this Vector2 a)
	{
		return new Vector2(Mathf.Abs(a.x), Mathf.Abs(a.y));
	}
}
