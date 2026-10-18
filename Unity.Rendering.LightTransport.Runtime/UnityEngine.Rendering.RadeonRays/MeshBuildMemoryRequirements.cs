namespace UnityEngine.Rendering.RadeonRays;

internal struct MeshBuildMemoryRequirements
{
	public ulong buildScratchSizeInDwords;

	public ulong bvhSizeInDwords;

	public ulong bvhLeavesSizeInDwords;
}
