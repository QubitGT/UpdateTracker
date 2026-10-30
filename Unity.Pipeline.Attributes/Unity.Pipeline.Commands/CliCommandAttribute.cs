using System;

namespace Unity.Pipeline.Commands;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CliCommandAttribute : Attribute
{
	public string Group { get; }

	public string Name { get; }

	public string Description { get; }

	public bool MainThreadRequired { get; set; } = true;


	public string[] Tags { get; set; } = Array.Empty<string>();


	public CliCommandAttribute(string group, string name, string description)
	{
		Group = group ?? throw new ArgumentNullException("group");
		Name = name ?? throw new ArgumentNullException("name");
		Description = description ?? throw new ArgumentNullException("description");
	}

	[Obsolete("Name the command's group: [CliCommand(group, name, description)]. With only a name the command is registered in the 'global' group.")]
	public CliCommandAttribute(string name, string description)
		: this("global", name, description)
	{
	}
}
