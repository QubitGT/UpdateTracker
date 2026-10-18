using System;
using System.Diagnostics;

namespace Fusion.Analyzer;

[AttributeUsage(AttributeTargets.Field)]
[Conditional("false")]
public class StaticFieldAttribute : Attribute
{
	public StaticFieldResetMode Reset { get; }

	public StaticFieldAttribute(StaticFieldResetMode resetMode)
	{
		Reset = resetMode;
	}

	public StaticFieldAttribute()
		: this(StaticFieldResetMode.ResetMethod)
	{
	}
}
