namespace UnityEngine.Rendering;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\GPUDriven\\OccluderDepthPyramidConstants.cs", needAccessors = false, generateCBuffer = true)]
internal struct OccluderDepthPyramidConstants
{
	[HLSLArray(6, typeof(Matrix4x4))]
	public unsafe fixed float _InvViewProjMatrix[96];

	[HLSLArray(6, typeof(Vector4))]
	public unsafe fixed float _SilhouettePlanes[24];

	[HLSLArray(6, typeof(ShaderGenUInt4))]
	public unsafe fixed uint _SrcOffset[24];

	[HLSLArray(5, typeof(ShaderGenUInt4))]
	public unsafe fixed uint _MipOffsetAndSize[20];

	public uint _OccluderMipLayoutSizeX;

	public uint _OccluderMipLayoutSizeY;

	public uint _OccluderDepthPyramidPad0;

	public uint _OccluderDepthPyramidPad1;

	public uint _SrcSliceIndices;

	public uint _DstSubviewIndices;

	public uint _MipCount;

	public uint _SilhouettePlaneCount;
}
