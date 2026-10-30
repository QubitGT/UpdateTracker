using System;

namespace Unity.Pipeline.Commands;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CliGroupCommandAttribute : Attribute
{
	public string Group { get; }

	public string Name { get; set; }

	public string Description { get; }

	public bool MainThreadRequired { get; set; } = true;


	public string[] Tags { get; set; } = Array.Empty<string>();


	public CliGroupCommandAttribute(string group, string description)
	{
		Group = group ?? throw new ArgumentNullException("group");
		Description = description ?? throw new ArgumentNullException("description");
	}
}
