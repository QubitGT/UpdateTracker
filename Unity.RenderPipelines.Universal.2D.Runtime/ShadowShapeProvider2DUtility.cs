using UnityEngine;

internal static class ShadowShapeProvider2DUtility
{
	public static float GetTrimEdgeFromBounds(Bounds bounds, float trimMultipler)
	{
		Vector3 size = bounds.size;
		float num = trimMultipler * ((size.x < size.y) ? size.x : size.y);
		float num2 = Mathf.Pow(10f, 0f - Mathf.Floor(Mathf.Log10(num)));
		return Mathf.Floor(num * num2) / num2;
	}

	public static bool IsUsingGpuDeformation()
	{
		return false;
	}
}
