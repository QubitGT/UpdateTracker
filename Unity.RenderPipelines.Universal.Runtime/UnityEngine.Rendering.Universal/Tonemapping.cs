using System;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[VolumeComponentMenu("Post-processing/Tonemapping")]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
public sealed class Tonemapping : VolumeComponent, IPostProcessComponent
{
	[Tooltip("Select a tonemapping algorithm to use for the color grading process.")]
	public TonemappingModeParameter mode = new TonemappingModeParameter(TonemappingMode.None);

	[AdditionalProperty]
	[Tooltip("Specifies the range reduction mode used when HDR output is enabled and Neutral tonemapping is enabled.")]
	public NeutralRangeReductionModeParameter neutralHDRRangeReductionMode = new NeutralRangeReductionModeParameter(NeutralRangeReductionMode.BT2390);

	[Tooltip("Use the ACES preset for HDR displays.")]
	public HDRACESPresetParameter acesPreset = new HDRACESPresetParameter(HDRACESPreset.ACES1000Nits);

	[Tooltip("Specify how much hue to preserve. Values closer to 0 are likely to preserve hue. As values get closer to 1, Unity doesn't correct hue shifts.")]
	public ClampedFloatParameter hueShiftAmount = new ClampedFloatParameter(0f, 0f, 1f);

	[Tooltip("Enable to use values detected from the output device as paper white. When enabled, output images might differ between SDR and HDR. For best accuracy, set this value manually.")]
	public BoolParameter detectPaperWhite = new BoolParameter(value: false);

	[Tooltip("The reference brightness of a paper white surface. This property determines the maximum brightness of UI. The brightness of the scene is scaled relative to this value. The value is in nits.")]
	public ClampedFloatParameter paperWhite = new ClampedFloatParameter(300f, 0f, 400f);

	[Tooltip("Enable to use the minimum and maximum brightness values detected from the output device. For best accuracy, considering calibrating these values manually.")]
	public BoolParameter detectBrightnessLimits = new BoolParameter(value: true);

	[Tooltip("The minimum brightness of the screen (in nits). This value is assumed to be 0.005f with ACES Tonemap.")]
	public ClampedFloatParameter minNits = new ClampedFloatParameter(0.005f, 0f, 50f);

	[Tooltip("The maximum brightness of the screen (in nits). This value is defined by the preset when using ACES Tonemap.")]
	public ClampedFloatParameter maxNits = new ClampedFloatParameter(1000f, 0f, 5000f);

	public bool IsActive()
	{
		return mode.value != TonemappingMode.None;
	}

	[Obsolete("Unused #from(2023.1)", false)]
	public bool IsTileCompatible()
	{
		return true;
	}
}
