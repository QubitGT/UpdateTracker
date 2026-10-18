using Unity.Mathematics;

namespace UnityEngine.Rendering;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\Lighting\\ProbeVolume\\ShaderVariablesProbeVolumes.cs", needAccessors = false, generateCBuffer = true, constantRegister = 6)]
internal struct ShaderVariablesProbeVolumes
{
	public Vector4 _Offset_LayerCount;

	public Vector4 _MinLoadedCellInEntries_IndirectionEntryDim;

	public Vector4 _MaxLoadedCellInEntries_RcpIndirectionEntryDim;

	public Vector4 _PoolDim_MinBrickSize;

	public Vector4 _RcpPoolDim_XY;

	public Vector4 _MinEntryPos_Noise;

	public uint4 _EntryCount_X_XY_LeakReduction;

	public Vector4 _Biases_NormalizationClamp;

	public Vector4 _FrameIndex_Weights;

	public uint4 _ProbeVolumeLayerMask;
}
