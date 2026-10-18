using System;

namespace VYaml.Annotations;

[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
public sealed class YamlConstructorAttribute : Attribute
{
}
