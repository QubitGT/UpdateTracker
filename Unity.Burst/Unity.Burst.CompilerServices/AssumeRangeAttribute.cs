using System;

namespace Unity.Burst.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
public class AssumeRangeAttribute : Attribute
{
	public AssumeRangeAttribute(long min, long max)
	{
	}

	public AssumeRangeAttribute(ulong min, ulong max)
	{
	}
}
