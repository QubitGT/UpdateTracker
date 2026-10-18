namespace UnityEngine.Rendering.RadeonRays;

internal class SceneMemoryRequirements
{
	public ulong buildScratchSizeInDwords;

	public ulong[] bottomLevelBvhSizeInNodes;

	public uint[] bottomLevelBvhOffsetInNodes;

	public ulong[] bottomLevelBvhLeavesSizeInNodes;

	public uint[] bottomLevelBvhLeavesOffsetInNodes;

	public ulong totalBottomLevelBvhSizeInNodes;

	public ulong totalBottomLevelBvhLeavesSizeInNodes;
}
