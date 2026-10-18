using System;

namespace VYaml.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface, Inherited = false)]
public class YamlObjectAttribute : Attribute
{
	public NamingConvention NamingConvention { get; }

	public YamlObjectAttribute(NamingConvention namingConvention = NamingConvention.LowerCamelCase)
	{
		NamingConvention = namingConvention;
	}
}
