using System;

namespace Unity.XR.CoreUtils;

[AttributeUsage(AttributeTargets.Class)]
public class ScriptableSettingsPathAttribute : Attribute
{
	private readonly string m_Path;

	public string Path => m_Path;

	public ScriptableSettingsPathAttribute(string path = "")
	{
		m_Path = path;
	}
}
