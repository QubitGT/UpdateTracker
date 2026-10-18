using System;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[VolumeComponentMenu("Post-processing/Color Curves")]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
public sealed class ColorCurves : VolumeComponent, IPostProcessComponent
{
	[Tooltip("Affects the luminance across the whole image.")]
	public TextureCurveParameter master;

	[Tooltip("Affects the red channel intensity across the whole image.")]
	public TextureCurveParameter red;

	[Tooltip("Affects the green channel intensity across the whole image.")]
	public TextureCurveParameter green;

	[Tooltip("Affects the blue channel intensity across the whole image.")]
	public TextureCurveParameter blue;

	[Tooltip("Shifts the input hue (x-axis) according to the output hue (y-axis).")]
	public TextureCurveParameter hueVsHue;

	[Tooltip("Adjusts saturation (y-axis) according to the input hue (x-axis).")]
	public TextureCurveParameter hueVsSat;

	[Tooltip("Adjusts saturation (y-axis) according to the input saturation (x-axis).")]
	public TextureCurveParameter satVsSat;

	[Tooltip("Adjusts saturation (y-axis) according to the input luminance (x-axis).")]
	public TextureCurveParameter lumVsSat;

	public bool IsActive()
	{
		return true;
	}

	[Obsolete("Unused #from(2023.1)", false)]
	public bool IsTileCompatible()
	{
		return true;
	}

	public ColorCurves()
	{
		Keyframe[] keys = new Keyframe[2]
		{
			new Keyframe(0f, 0f, 1f, 1f),
			new Keyframe(1f, 1f, 1f, 1f)
		};
		Vector2 bounds = new Vector2(0f, 1f);
		master = new TextureCurveParameter(new TextureCurve(keys, 0f, loop: false, in bounds));
		Keyframe[] keys2 = new Keyframe[2]
		{
			new Keyframe(0f, 0f, 1f, 1f),
			new Keyframe(1f, 1f, 1f, 1f)
		};
		bounds = new Vector2(0f, 1f);
		red = new TextureCurveParameter(new TextureCurve(keys2, 0f, loop: false, in bounds));
		Keyframe[] keys3 = new Keyframe[2]
		{
			new Keyframe(0f, 0f, 1f, 1f),
			new Keyframe(1f, 1f, 1f, 1f)
		};
		bounds = new Vector2(0f, 1f);
		green = new TextureCurveParameter(new TextureCurve(keys3, 0f, loop: false, in bounds));
		Keyframe[] keys4 = new Keyframe[2]
		{
			new Keyframe(0f, 0f, 1f, 1f),
			new Keyframe(1f, 1f, 1f, 1f)
		};
		bounds = new Vector2(0f, 1f);
		blue = new TextureCurveParameter(new TextureCurve(keys4, 0f, loop: false, in bounds));
		Keyframe[] keys5 = new Keyframe[0];
		bounds = new Vector2(0f, 1f);
		hueVsHue = new TextureCurveParameter(new TextureCurve(keys5, 0.5f, loop: true, in bounds));
		Keyframe[] keys6 = new Keyframe[0];
		bounds = new Vector2(0f, 1f);
		hueVsSat = new TextureCurveParameter(new TextureCurve(keys6, 0.5f, loop: true, in bounds));
		Keyframe[] keys7 = new Keyframe[0];
		bounds = new Vector2(0f, 1f);
		satVsSat = new TextureCurveParameter(new TextureCurve(keys7, 0.5f, loop: false, in bounds));
		Keyframe[] keys8 = new Keyframe[0];
		bounds = new Vector2(0f, 1f);
		lumVsSat = new TextureCurveParameter(new TextureCurve(keys8, 0.5f, loop: false, in bounds));
		base._002Ector();
	}
}
