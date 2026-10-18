using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Class)]
public class FusionGlobalScriptableObjectAttribute : Attribute
{
	public string DefaultPath { get; }

	public string DefaultContents { get; set; }

	public string DefaultContentsGeneratorMethod { get; set; }

	public FusionGlobalScriptableObjectAttribute(string defaultPath)
	{
		DefaultPath = defaultPath;
	}
}
