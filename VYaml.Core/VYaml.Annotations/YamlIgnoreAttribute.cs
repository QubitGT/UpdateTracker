using System;

namespace VYaml.Annotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class YamlIgnoreAttribute : Attribute
{
}
