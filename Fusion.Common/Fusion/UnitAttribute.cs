using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Field)]
public sealed class UnitAttribute : DecoratingPropertyAttribute
{
	public Units Unit { get; }

	public UnitAttribute(Units units)
	{
		Unit = units;
	}
}
