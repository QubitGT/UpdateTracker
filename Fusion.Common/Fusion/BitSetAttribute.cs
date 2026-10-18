using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class BitSetAttribute : DrawerPropertyAttribute
{
	public int BitCount { get; }

	public BitSetAttribute(int bitCount)
	{
		BitCount = bitCount;
	}
}
