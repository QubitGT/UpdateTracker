namespace UnityEngine.Rendering.Universal;

public enum AntialiasingMode
{
	[InspectorName("No Anti-aliasing")]
	None,
	[InspectorName("Fast Approximate Anti-aliasing (FXAA)")]
	FastApproximateAntialiasing,
	[InspectorName("Subpixel Morphological Anti-aliasing (SMAA)")]
	SubpixelMorphologicalAntiAliasing,
	[InspectorName("Temporal Anti-aliasing (TAA)")]
	TemporalAntiAliasing
}
