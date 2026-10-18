using System;
using Unity.XR.CoreUtils.Collections;

namespace Unity.XR.CoreUtils.Capabilities;

[Serializable]
public sealed class CapabilityDictionary : SerializableDictionary<string, bool>
{
	public void ForceSerialize()
	{
		base.OnBeforeSerialize();
	}

	public override void OnBeforeSerialize()
	{
	}
}
