namespace UnityEngine.Rendering;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\GPUDriven\\InstanceOcclusionCullerShaderVariables.cs", needAccessors = false, generateCBuffer = true)]
internal struct InstanceOcclusionCullerShaderVariables
{
	public uint _DrawInfoAllocIndex;

	public uint _DrawInfoCount;

	public uint _InstanceInfoAllocIndex;

	public uint _InstanceInfoCount;

	public int _BoundingSphereInstanceDataAddress;

	public int _DebugCounterIndex;

	public int _InstanceMultiplierShift;

	public int _InstanceOcclusionCullerPad0;
}
