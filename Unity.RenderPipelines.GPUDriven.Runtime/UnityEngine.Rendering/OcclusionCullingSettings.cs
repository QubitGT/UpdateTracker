namespace UnityEngine.Rendering;

public struct OcclusionCullingSettings
{
	public int viewInstanceID;

	public OcclusionTest occlusionTest;

	public int instanceMultiplier;

	public OcclusionCullingSettings(int viewInstanceID, OcclusionTest occlusionTest)
	{
		this.viewInstanceID = viewInstanceID;
		this.occlusionTest = occlusionTest;
		instanceMultiplier = 1;
	}
}
