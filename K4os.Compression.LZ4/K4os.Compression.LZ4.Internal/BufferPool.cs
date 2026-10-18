using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace K4os.Compression.LZ4.Internal;

public static class BufferPool
{
	public const int MinPooledSize = 512;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool ShouldBePooled(int length)
	{
		return length >= 512;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte[] Rent(int size, bool zero)
	{
		byte[] array = ArrayPool<byte>.Shared.Rent(size);
		if (zero)
		{
			array.AsSpan(0, size).Clear();
		}
		return array;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte[] Alloc(int size, bool zero = false)
	{
		if (!ShouldBePooled(size))
		{
			return new byte[size];
		}
		return Rent(size, zero);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPooled(byte[] buffer)
	{
		return ShouldBePooled(buffer.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Free(byte[]? buffer)
	{
		if (buffer != null && IsPooled(buffer))
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}
}
