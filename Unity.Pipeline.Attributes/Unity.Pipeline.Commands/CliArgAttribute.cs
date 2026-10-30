using System;

namespace Unity.Pipeline.Commands;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class CliArgAttribute : Attribute
{
	public string Name { get; }

	public string Description { get; }

	public bool Required { get; set; }

	public object DefaultValue { get; set; }

	public CliArgAttribute(string name, string description)
	{
		Name = name ?? throw new ArgumentNullException("name");
		Description = description ?? throw new ArgumentNullException("description");
	}
}
