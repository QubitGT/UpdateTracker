namespace UnityEngine.Rendering;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\PostProcessing\\HDROutputDefines.cs")]
public enum HDRRangeReduction
{
	None,
	Reinhard,
	BT2390,
	ACES1000Nits,
	ACES2000Nits,
	ACES4000Nits
}
