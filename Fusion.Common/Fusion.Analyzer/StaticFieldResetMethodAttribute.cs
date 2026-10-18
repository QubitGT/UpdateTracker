using System;
using System.Diagnostics;

namespace Fusion.Analyzer;

[AttributeUsage(AttributeTargets.Method)]
[Conditional("false")]
public class StaticFieldResetMethodAttribute : Attribute
{
	public StaticFieldResetMethodAttribute(bool calledAutomatically)
	{
	}

	public StaticFieldResetMethodAttribute()
	{
	}
}
