using UnityEngine;

namespace Unity.XR.CoreUtils;

public static class Vector3Extensions
{
	public static Vector3 Inverse(this Vector3 vector)
	{
		return new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
	}

	public static float MinComponent(this Vector3 vector)
	{
		return Mathf.Min(Mathf.Min(vector.x, vector.y), vector.z);
	}

	public static float MaxComponent(this Vector3 vector)
	{
		return Mathf.Max(Mathf.Max(vector.x, vector.y), vector.z);
	}

	public static Vector3 Abs(this Vector3 vector)
	{
		vector.x = Mathf.Abs(vector.x);
		vector.y = Mathf.Abs(vector.y);
		vector.z = Mathf.Abs(vector.z);
		return vector;
	}

	public static Vector3 Multiply(this Vector3 value, Vector3 scale)
	{
		return new Vector3(value.x * scale.x, value.y * scale.y, value.z * scale.z);
	}

	public static Vector3 Divide(this Vector3 value, Vector3 scale)
	{
		return new Vector3(value.x / scale.x, value.y / scale.y, value.z / scale.z);
	}

	public static Vector3 SafeDivide(this Vector3 value, Vector3 scale)
	{
		float num = (Mathf.Approximately(scale.x, 0f) ? 0f : (value.x / scale.x));
		if (float.IsNaN(num))
		{
			num = 0f;
		}
		float num2 = (Mathf.Approximately(scale.y, 0f) ? 0f : (value.y / scale.y));
		if (float.IsNaN(num2))
		{
			num2 = 0f;
		}
		float num3 = (Mathf.Approximately(scale.z, 0f) ? 0f : (value.z / scale.z));
		if (float.IsNaN(num3))
		{
			num3 = 0f;
		}
		return new Vector3(num, num2, num3);
	}
}
