namespace UnityEngine.Rendering.Universal;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\ShaderLibrary\\Debug\\DebugViewEnums.cs")]
public enum DebugFullScreenMode
{
	None,
	Depth,
	[InspectorName("Motion Vector (100x, normalized)")]
	MotionVector,
	AdditionalLightsShadowMap,
	MainLightShadowMap,
	AdditionalLightsCookieAtlas,
	ReflectionProbeAtlas,
	STP
}
