namespace UnityEngine.ProBuilder;

internal static class VectorHash
{
	public const float FltCompareResolution = 1000f;

	private static int HashFloat(float f)
	{
		return (int)((ulong)(f * 1000f) % int.MaxValue);
	}

	public static int GetHashCode(Vector2 v)
	{
		return (27 * 29 + HashFloat(v.x)) * 29 + HashFloat(v.y);
	}

	public static int GetHashCode(Vector3 v)
	{
		return ((27 * 29 + HashFloat(v.x)) * 29 + HashFloat(v.y)) * 29 + HashFloat(v.z);
	}

	public static int GetHashCode(Vector4 v)
	{
		return (((27 * 29 + HashFloat(v.x)) * 29 + HashFloat(v.y)) * 29 + HashFloat(v.z)) * 29 + HashFloat(v.w);
	}
}
