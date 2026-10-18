using System.Threading;
using Unity.Mathematics;

namespace Unity.Collections;

internal class ConcurrentMask
{
	internal const int ErrorFailedToFree = -1;

	internal const int ErrorFailedToAllocate = -2;

	internal const int ErrorAllocationCrossesWordBoundary = -3;

	internal const int EmptyBeforeAllocation = 0;

	internal const int EmptyAfterFree = 0;

	internal static long AtomicOr(ref long destination, long source)
	{
		long num = Interlocked.Read(ref destination);
		long num2;
		long num3;
		do
		{
			num2 = num | source;
			num3 = num;
			num = Interlocked.CompareExchange(ref destination, num2, num3);
		}
		while (num != num3);
		return num2;
	}

	internal static long AtomicAnd(ref long destination, long source)
	{
		long num = Interlocked.Read(ref destination);
		long num2;
		long num3;
		do
		{
			num2 = num & source;
			num3 = num;
			num = Interlocked.CompareExchange(ref destination, num2, num3);
		}
		while (num != num3);
		return num2;
	}

	internal static void longestConsecutiveOnes(long value, out int offset, out int count)
	{
		count = 0;
		long num = value;
		while (num != 0L)
		{
			value = num;
			num = value & (value >>> 1);
			count++;
		}
		offset = math.tzcnt(value);
	}

	internal static bool foundAtLeastThisManyConsecutiveOnes(long value, int minimum, out int offset, out int count)
	{
		if (minimum == 1)
		{
			offset = math.tzcnt(value);
			count = 1;
			return offset != 64;
		}
		longestConsecutiveOnes(value, out offset, out count);
		return count >= minimum;
	}

	internal static bool foundAtLeastThisManyConsecutiveZeroes(long value, int minimum, out int offset, out int count)
	{
		return foundAtLeastThisManyConsecutiveOnes(~value, minimum, out offset, out count);
	}

	internal static bool Succeeded(int error)
	{
		return error >= 0;
	}

	internal static long MakeMask(int offset, int bits)
	{
		return (long)(ulong.MaxValue >> 64 - bits << offset);
	}

	internal static int TryAllocate(ref long l, int offset, int bits)
	{
		long num = MakeMask(offset, bits);
		long num2 = Interlocked.Read(ref l);
		long num3;
		do
		{
			if ((num2 & num) != 0L)
			{
				return -2;
			}
			long value = num2 | num;
			num3 = num2;
			num2 = Interlocked.CompareExchange(ref l, value, num3);
		}
		while (num2 != num3);
		return math.countbits(num2);
	}

	internal static int TryFree(ref long l, int offset, int bits)
	{
		long num = MakeMask(offset, bits);
		long num2 = Interlocked.Read(ref l);
		long num3;
		long num4;
		do
		{
			if ((num2 & num) != num)
			{
				return -1;
			}
			num3 = num2 & ~num;
			num4 = num2;
			num2 = Interlocked.CompareExchange(ref l, num3, num4);
		}
		while (num2 != num4);
		return math.countbits(num3);
	}

	internal static int TryAllocate(ref long l, out int offset, int bits)
	{
		long num = Interlocked.Read(ref l);
		long num3;
		do
		{
			if (!foundAtLeastThisManyConsecutiveZeroes(num, bits, out offset, out var _))
			{
				return -2;
			}
			long num2 = MakeMask(offset, bits);
			long value = num | num2;
			num3 = num;
			num = Interlocked.CompareExchange(ref l, value, num3);
		}
		while (num != num3);
		return math.countbits(num);
	}

	internal static int TryAllocate<T>(ref T t, int offset, int bits) where T : IIndexable<long>
	{
		int index = offset >> 6;
		int num = offset & 0x3F;
		if (num + bits > 64)
		{
			return -3;
		}
		return TryAllocate(ref t.ElementAt(index), num, bits);
	}

	internal static int TryFree<T>(ref T t, int offset, int bits) where T : IIndexable<long>
	{
		int index = offset >> 6;
		int offset2 = offset & 0x3F;
		return TryFree(ref t.ElementAt(index), offset2, bits);
	}

	internal static int TryAllocate<T>(ref T t, out int offset, int begin, int end, int bits) where T : IIndexable<long>
	{
		int i;
		for (i = begin; i < end && t.ElementAt(i) == -1; i++)
		{
		}
		for (; i < end; i++)
		{
			int offset2;
			int num = TryAllocate(ref t.ElementAt(i), out offset2, bits);
			if (Succeeded(num))
			{
				offset = i * 64 + offset2;
				return num;
			}
		}
		offset = -1;
		return -2;
	}

	internal static int TryAllocate<T>(ref T t, out int offset, int begin, int bits) where T : IIndexable<long>
	{
		int num = TryAllocate(ref t, out offset, begin, t.Length, bits);
		if (Succeeded(num))
		{
			return num;
		}
		return TryAllocate(ref t, out offset, 0, begin, bits);
	}

	internal static int TryAllocate<T>(ref T t, out int offset, int bits) where T : IIndexable<long>
	{
		return TryAllocate(ref t, out offset, 0, t.Length, bits);
	}
}
