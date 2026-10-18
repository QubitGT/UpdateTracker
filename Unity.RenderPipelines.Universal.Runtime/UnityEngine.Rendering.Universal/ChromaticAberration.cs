using System;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[VolumeComponentMenu("Post-processing/Chromatic Aberration")]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
public sealed class ChromaticAberration : VolumeComponent, IPostProcessComponent
{
	[Tooltip("Use the slider to set the strength of the Chromatic Aberration effect.")]
	public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

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
