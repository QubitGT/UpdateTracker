using Unity.Burst;

namespace Drawing;

public static class SharedDrawingData
{
	private class BurstTimeKey
	{
	}

	public static readonly SharedStatic<float> BurstTime = SharedStatic<float>.GetOrCreateUnsafe(4u, 527447541831459905L, -5918529866343830416L);
}
