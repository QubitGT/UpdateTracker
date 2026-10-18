namespace UnityEngine.Rendering;

internal struct LODGroupData
{
	public const int k_MaxLODLevelsCount = 8;

	public bool valid;

	public int lodCount;

	public int rendererCount;

	public unsafe fixed float screenRelativeTransitionHeights[8];

	public unsafe fixed float fadeTransitionWidth[8];
}
