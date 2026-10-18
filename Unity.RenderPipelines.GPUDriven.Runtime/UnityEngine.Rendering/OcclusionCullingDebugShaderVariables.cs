namespace UnityEngine.Rendering;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\GPUDriven\\OcclusionCullingDebugShaderVariables.cs", needAccessors = false, generateCBuffer = true)]
internal struct OcclusionCullingDebugShaderVariables
{
	public Vector4 _DepthSizeInOccluderPixels;

	[HLSLArray(8, typeof(ShaderGenUInt4))]
	public unsafe fixed uint _OccluderMipBounds[32];

	public uint _OccluderMipLayoutSizeX;

	public uint _OccluderMipLayoutSizeY;

	public uint _OcclusionCullingDebugPad0;

	public uint _OcclusionCullingDebugPad1;
}
