namespace UnityEngine.Rendering.Universal;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\ShaderLibrary\\Debug\\DebugViewEnums.cs")]
public enum DebugValidationMode
{
	None,
	[InspectorName("Highlight NaN, Inf and Negative Values")]
	HighlightNanInfNegative,
	[InspectorName("Highlight Values Outside Range")]
	HighlightOutsideOfRange
}
