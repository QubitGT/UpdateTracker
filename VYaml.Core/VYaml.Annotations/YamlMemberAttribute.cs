using System;

namespace VYaml.Annotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class YamlMemberAttribute : Attribute
{
	public string? Name { get; }

	public int Order { get; set; }

	public YamlMemberAttribute(string? name = null)
	{
		Name = name;
	}
}
