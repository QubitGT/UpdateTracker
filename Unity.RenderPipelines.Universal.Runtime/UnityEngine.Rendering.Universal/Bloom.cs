using System;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[VolumeComponentMenu("Post-processing/Bloom")]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
public sealed class Bloom : VolumeComponent, IPostProcessComponent
{
	[Obsolete("This is obsolete, please use maxIterations instead.", true)]
	[Tooltip("The number of final iterations to skip in the effect processing sequence.")]
	public ClampedIntParameter skipIterations = new ClampedIntParameter(1, 0, 16);

	[Header("Bloom")]
	[Tooltip("Filters out pixels under this level of brightness. Value is in gamma-space.")]
	public MinFloatParameter threshold = new MinFloatParameter(0.9f, 0f);

	[Tooltip("Strength of the bloom filter.")]
	public MinFloatParameter intensity = new MinFloatParameter(0f, 0f);

	[Tooltip("Set the radius of the bloom effect.")]
	public ClampedFloatParameter scatter = new ClampedFloatParameter(0.7f, 0f, 1f);

	[Tooltip("Set the maximum intensity that Unity uses to calculate Bloom. If pixels in your Scene are more intense than this, URP renders them at their current intensity, but uses this intensity value for the purposes of Bloom calculations.")]
	public MinFloatParameter clamp = new MinFloatParameter(65472f, 0f);

	[Tooltip("Use the color picker to select a color for the Bloom effect to tint to.")]
	public ColorParameter tint = new ColorParameter(Color.white, hdr: false, showAlpha: false, showEyeDropper: true);

	[Tooltip("Use bicubic sampling instead of bilinear sampling for the upsampling passes. This is slightly more expensive but helps getting smoother visuals.")]
	public BoolParameter highQualityFiltering = new BoolParameter(value: false);

	[Tooltip("The starting resolution that this effect begins processing.")]
	[AdditionalProperty]
	public DownscaleParameter downscale = new DownscaleParameter(BloomDownscaleMode.Half);

	[Tooltip("The maximum number of iterations in the effect processing sequence.")]
	[AdditionalProperty]
	public ClampedIntParameter maxIterations = new ClampedIntParameter(6, 2, 8);

	[Header("Lens Dirt")]
	[Tooltip("Dirtiness texture to add smudges or dust to the bloom effect.")]
	public TextureParameter dirtTexture = new TextureParameter(null);

	[Tooltip("Amount of dirtiness.")]
	public MinFloatParameter dirtIntensity = new MinFloatParameter(0f, 0f);

	public bool IsActive()
	{
		return intensity.value > 0f;
	}

	[Obsolete("Unused #from(2023.1)", false)]
	public bool IsTileCompatible()
	{
		return false;
	}
}
