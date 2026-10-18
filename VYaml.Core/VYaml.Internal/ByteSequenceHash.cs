using System;

namespace VYaml.Internal;

public static class ByteSequenceHash
{
	public static int GetHashCode(ReadOnlySpan<byte> span)
	{
		uint num = 2166136261u;
		ReadOnlySpan<byte> readOnlySpan = span;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			num = (readOnlySpan[i] ^ num) * 16777619;
		}
		return (int)num;
	}
}
