namespace UnityEngine.Rendering;

public static class OcclusionTestMethods
{
	public static uint GetBatchLayerMask(this OcclusionTest occlusionTest)
	{
		if (occlusionTest != OcclusionTest.TestCulled)
		{
			return uint.MaxValue;
		}
		return 268435456u;
	}
}
