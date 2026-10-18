namespace UnityEngine.Rendering.UnifiedRayTracing;

internal class ReferenceCounter
{
	public ulong value;

	public void Inc()
	{
		value++;
	}

	public void Dec()
	{
		value--;
	}
}
