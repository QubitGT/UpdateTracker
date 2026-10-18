using System;

namespace VYaml.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public class YamlObjectUnionAttribute : Attribute
{
	public string Tag { get; }

	public Type SubType { get; }

	public YamlObjectUnionAttribute(string tagString, Type subType)
	{
		Tag = tagString;
		SubType = subType;
	}
}
