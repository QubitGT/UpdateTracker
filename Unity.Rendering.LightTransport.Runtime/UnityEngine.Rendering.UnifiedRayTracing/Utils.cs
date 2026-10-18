namespace UnityEngine.Rendering.UnifiedRayTracing;

internal static class Utils
{
	public static void Destroy(Object obj)
	{
		if (obj != null)
		{
			Object.Destroy(obj);
		}
	}
}
