using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class NetworkedAttribute : Attribute
{
	public string Default { get; set; }
}
