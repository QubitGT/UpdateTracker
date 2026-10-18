using System;

namespace UnityEngine.Rendering;

[Serializable]
[VolumeComponentMenu("Lighting/Adaptive Probe Volumes Options")]
[SupportedOnRenderPipeline(new Type[] { })]
public sealed class ProbeVolumesOptions : VolumeComponent
{
	[Tooltip("The overridden normal bias to be applied to the world position when sampling the Adaptive Probe Volumes data structure. Unit is meters.")]
	public ClampedFloatParameter normalBias = new ClampedFloatParameter(0.05f, 0f, 2f);

	[Tooltip("A bias alongside the view vector to be applied to the world position when sampling the Adaptive Probe Volumes data structure. Unit is meters.")]
	public ClampedFloatParameter viewBias = new ClampedFloatParameter(0.1f, 0f, 2f);

	[Tooltip("Whether to scale the bias for Adaptive Probe Volumes by the minimum distance between probes.")]
	public BoolParameter scaleBiasWithMinProbeDistance = new BoolParameter(value: false);

	[Tooltip("Noise to be applied to the sampling position. It can hide seams issues between subdivision levels, but introduces noise.")]
	public ClampedFloatParameter samplingNoise = new ClampedFloatParameter(0.1f, 0f, 1f);

	[Tooltip("Whether to animate the noise when TAA is enabled. It can potentially remove the visible noise patterns.")]
	public BoolParameter animateSamplingNoise = new BoolParameter(value: true);

	[Tooltip("Method used to reduce leaks. Currently available modes are crude, but cheap methods.")]
	public APVLeakReductionModeParameter leakReductionMode = new APVLeakReductionModeParameter(APVLeakReductionMode.Quality);

	[Obsolete("This parameter isn't used anymore.")]
	public ClampedFloatParameter minValidDotProductValue = new ClampedFloatParameter(0.1f, -1f, 0.33f);

	[Tooltip("When enabled, reflection probe normalization can only decrease the reflection intensity.")]
	public BoolParameter occlusionOnlyReflectionNormalization = new BoolParameter(value: true);

	[AdditionalProperty]
	[Tooltip("Global probe volumes weight. Allows for fading out probe volumes influence falling back to ambient probe.")]
	public ClampedFloatParameter intensityMultiplier = new ClampedFloatParameter(1f, 0f, 1f);

	[AdditionalProperty]
	[Tooltip("Multiplier applied on the sky lighting when using sky occlusion.")]
	public ClampedFloatParameter skyOcclusionIntensityMultiplier = new ClampedFloatParameter(1f, 0f, 5f);

	[AdditionalProperty]
	[Tooltip("Offset applied at runtime to probe positions in world space.\nThis is not considered while baking.")]
	public Vector3Parameter worldOffset = new Vector3Parameter(Vector3.zero);

	private ProbeVolumesOptions()
	{
		base.displayName = "Adaptive Probe Volumes Options";
	}
}
