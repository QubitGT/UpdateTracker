using System;
using UnityEngine;

namespace Unity.XR.CoreUtils.Capabilities;

public abstract class CapabilityProfile : ScriptableObject
{
	public static event Action<CapabilityProfile> CapabilityChanged;

	public void ReportCapabilityChanged()
	{
		CapabilityProfile.CapabilityChanged?.Invoke(this);
	}
}
