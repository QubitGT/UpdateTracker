using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class NetworkSerializeMethodAttribute : Attribute
{
	[Obsolete("No longer used. Use a method that returns a struct instead.", true)]
	public int MaxSize { get; set; }
}
