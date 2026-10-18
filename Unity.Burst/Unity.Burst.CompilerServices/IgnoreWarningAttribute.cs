using System;

namespace Unity.Burst.CompilerServices;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class IgnoreWarningAttribute : Attribute
{
	public IgnoreWarningAttribute(int warning)
	{
	}
}
