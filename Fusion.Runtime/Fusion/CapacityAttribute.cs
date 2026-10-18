using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public sealed class CapacityAttribute : Attribute
{
	public int Length { get; }

	public CapacityAttribute(int length)
	{
		Length = length;
	}
}
