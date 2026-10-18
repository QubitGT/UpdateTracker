using System;
using System.Diagnostics;

namespace UnityEngine.Rendering;

[Serializable]
[DebuggerDisplay("{m_Value} ({m_OverrideState})")]
public class MaterialParameter : VolumeParameter<Material>
{
	public MaterialParameter(Material value, bool overrideState = false)
		: base(value, overrideState)
	{
	}
}
