using System;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[VolumeComponentMenu("Post-processing/Screen Space Lens Flare")]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
public class ScreenSpaceLensFlare : VolumeComponent, IPostProcessComponent
{
	public MinFloatParameter intensity = new MinFloatParameter(0f, 0f);

	public ColorParameter tintColor = new ColorParameter(Color.white);

	[AdditionalProperty]
	public ClampedIntParameter bloomMip = new ClampedIntParameter(1, 0, 5);

	[Header("Flares")]
	public MinFloatParameter firstFlareIntensity = new MinFloatParameter(1f, 0f);

	public MinFloatParameter secondaryFlareIntensity = new MinFloatParameter(1f, 0f);

	public MinFloatParameter warpedFlareIntensity = new MinFloatParameter(1f, 0f);

	[AdditionalProperty]
	public Vector2Parameter warpedFlareScale = new Vector2Parameter(new Vector2(1f, 1f));

	public ClampedIntParameter samples = new ClampedIntParameter(1, 1, 3);

	[AdditionalProperty]
	public ClampedFloatParameter sampleDimmer = new ClampedFloatParameter(0.5f, 0.1f, 1f);

	public ClampedFloatParameter vignetteEffect = new ClampedFloatParameter(1f, 0f, 1f);

	public ClampedFloatParameter startingPosition = new ClampedFloatParameter(1.25f, 1f, 3f);

	public ClampedFloatParameter scale = new ClampedFloatParameter(1.5f, 1f, 4f);

	[Header("Streaks")]
	public MinFloatParameter streaksIntensity = new MinFloatParameter(0f, 0f);

	public ClampedFloatParameter streaksLength = new ClampedFloatParameter(0.5f, 0f, 1f);

	public FloatParameter streaksOrientation = new FloatParameter(0f);

	public ClampedFloatParameter streaksThreshold = new ClampedFloatParameter(0.25f, 0f, 1f);

	[SerializeField]
	[AdditionalProperty]
	public ScreenSpaceLensFlareResolutionParameter resolution = new ScreenSpaceLensFlareResolutionParameter(ScreenSpaceLensFlareResolution.Quarter);

	[Header("Chromatic Abberation")]
	public ClampedFloatParameter chromaticAbberationIntensity = new ClampedFloatParameter(0.5f, 0f, 1f);

	public ScreenSpaceLensFlare()
	{
		base.displayName = "Screen Space Lens Flare";
	}

	public bool IsActive()
	{
		return intensity.value > 0f;
	}

	public bool IsStreaksActive()
	{
		return streaksIntensity.value > 0f;
	}

	[Obsolete("Unused #from(2023.1)", false)]
	public bool IsTileCompatible()
	{
		return false;
	}
}
