using System;

namespace UnityEngine.Rendering;

[Serializable]
public sealed class APVLeakReductionModeParameter : VolumeParameter<APVLeakReductionMode>
{
	public APVLeakReductionModeParameter(APVLeakReductionMode value, bool overrideState = false)
		: base(value, overrideState)
	{
	}
}
