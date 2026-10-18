using System;

namespace UnityEngine.XR.Management;

[AttributeUsage(AttributeTargets.Class)]
public sealed class XRConfigurationDataAttribute : Attribute
{
	public string displayName { get; set; }

	public string buildSettingsKey { get; set; }

	private XRConfigurationDataAttribute()
	{
	}

	public XRConfigurationDataAttribute(string displayName, string buildSettingsKey)
	{
		this.displayName = displayName;
		this.buildSettingsKey = buildSettingsKey;
	}
}
