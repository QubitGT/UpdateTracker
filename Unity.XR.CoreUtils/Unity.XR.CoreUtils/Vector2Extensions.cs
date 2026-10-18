using UnityEngine;

namespace Unity.XR.CoreUtils;

public static class Vector2Extensions
{
	public static Vector2 Inverse(this Vector2 vector)
	{
		return new Vector2(1f / vector.x, 1f / vector.y);
	}

	public static float MinComponent(this Vector2 vector)
	{
		return Mathf.Min(vector.x, vector.y);
	}

	public static float MaxComponent(this Vector2 vector)
	{
		return Mathf.Max(vector.x, vector.y);
	}

	public static Vector2 Abs(this Vector2 vector)
	{
		vector.x = Mathf.Abs(vector.x);
		vector.y = Mathf.Abs(vector.y);
		return vector;
	}
}
