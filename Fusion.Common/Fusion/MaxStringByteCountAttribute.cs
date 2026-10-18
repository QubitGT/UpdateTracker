using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class MaxStringByteCountAttribute : DrawerPropertyAttribute
{
	public int ByteCount { get; }

	public string Encoding { get; }

	public MaxStringByteCountAttribute(int count, string encoding)
	{
		ByteCount = count;
		Encoding = encoding;
	}
}
