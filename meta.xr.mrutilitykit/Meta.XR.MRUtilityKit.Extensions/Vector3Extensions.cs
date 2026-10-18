using UnityEngine;

namespace Meta.XR.MRUtilityKit.Extensions;

internal static class Vector3Extensions
{
	internal static Vector3 Add(this Vector3 a, float b)
	{
		return new Vector3(a.x + b, a.y + b, a.z + b);
	}

	internal static Vector3 Subtract(this Vector3 a, float b)
	{
		return new Vector3(a.x - b, a.y - b, a.z - b);
	}

	internal static Vector3 Floor(this Vector3 a)
	{
		return new Vector3(Mathf.Floor(a.x), Mathf.Floor(a.y), Mathf.Floor(a.z));
	}

	internal static Vector3 FromVector2AndZ(Vector2 xy, float z)
	{
		return new Vector3(xy.x, xy.y, z);
	}
}
