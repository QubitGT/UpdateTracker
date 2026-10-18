namespace UnityEngine.Rendering;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\GPUDriven\\InstanceOcclusionCuller.cs", needAccessors = false)]
internal struct IndirectDrawInfo
{
	public uint indexCount;

	public uint firstIndex;

	public uint baseVertex;

	public uint firstInstanceGlobalIndex;

	public uint maxInstanceCountAndTopology;
}
