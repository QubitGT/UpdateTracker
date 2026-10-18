using System;

namespace UnityEngine.Rendering.Universal;

[Serializable]
public sealed class NeutralRangeReductionModeParameter : VolumeParameter<NeutralRangeReductionMode>
{
	public NeutralRangeReductionModeParameter(NeutralRangeReductionMode value, bool overrideState = false)
		: base(value, overrideState)
	{
	}
}
