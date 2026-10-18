using System;

namespace UnityEngine.Rendering.Universal;

[Serializable]
public sealed class DownscaleParameter : VolumeParameter<BloomDownscaleMode>
{
	public DownscaleParameter(BloomDownscaleMode value, bool overrideState = false)
		: base(value, overrideState)
	{
	}
}
