using System;
using System.Diagnostics;

namespace UnityEngine.Rendering;

[Serializable]
[DebuggerDisplay("{m_Value} ({m_OverrideState})")]
public sealed class EnumParameter<T> : VolumeParameter<T>
{
	public EnumParameter(T value, bool overrideState = false)
		: base(value, overrideState)
	{
	}
}
