using Unity.Mathematics;

namespace UnityEngine.Rendering;

internal struct ProbeVolumeShadingParameters
{
	public float normalBias;

	public float viewBias;

	public bool scaleBiasByMinDistanceBetweenProbes;

	public float samplingNoise;

	public float weight;

	public APVLeakReductionMode leakReductionMode;

	public int frameIndexForNoise;

	public float reflNormalizationLowerClamp;

	public float reflNormalizationUpperClamp;

	public float skyOcclusionIntensity;

	public bool skyOcclusionShadingDirection;

	public int regionCount;

	public uint4 regionLayerMasks;

	public Vector3 worldOffset;
}
