using System;

namespace UnityEngine.Rendering.Universal;

[Serializable]
public sealed class ScreenSpaceLensFlareResolutionParameter : VolumeParameter<ScreenSpaceLensFlareResolution>
{
	public ScreenSpaceLensFlareResolutionParameter(ScreenSpaceLensFlareResolution value, bool overrideState = false)
		: base(value, overrideState)
	{
	}
}
