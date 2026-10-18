using System.Threading;
using Unity.Mathematics;

namespace Unity.Collections.LowLevel.Unsafe;

[GenerateTestsForBurstCompatibility]
public struct UnsafeAtomicCounter64
{
	public unsafe long* Counter;

	public unsafe UnsafeAtomicCounter64(void* ptr)
	{
		Counter = (long*)ptr;
	}

	public unsafe void Reset(long value = 0L)
	{
		*Counter = value;
	}

	public unsafe long Add(long value)
	{
		return Interlocked.Add(ref UnsafeUtility.AsRef<long>(Counter), value) - value;
	}

	public long Sub(long value)
	{
		return Add(-value);
	}

	public unsafe long AddSat(long value, long max = long.MaxValue)
	{
		long num = *Counter;
		long num2;
		do
		{
			num2 = num;
			num = ((num >= max) ? max : math.min(max, num + value));
			num = Interlocked.CompareExchange(ref UnsafeUtility.AsRef<long>(Counter), num, num2);
		}
		while (num2 != num && num2 != max);
		return num2;
	}

	public unsafe long SubSat(long value, long min = long.MinValue)
	{
		long num = *Counter;
		long num2;
		do
		{
			num2 = num;
			num = ((num <= min) ? min : math.max(min, num - value));
			num = Interlocked.CompareExchange(ref UnsafeUtility.AsRef<long>(Counter), num, num2);
		}
		while (num2 != num && num2 != min);
		return num2;
	}
}
