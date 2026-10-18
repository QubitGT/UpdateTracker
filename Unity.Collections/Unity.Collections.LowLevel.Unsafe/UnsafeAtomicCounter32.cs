using System.Threading;
using Unity.Mathematics;

namespace Unity.Collections.LowLevel.Unsafe;

[GenerateTestsForBurstCompatibility]
public struct UnsafeAtomicCounter32
{
	public unsafe int* Counter;

	public unsafe UnsafeAtomicCounter32(void* ptr)
	{
		Counter = (int*)ptr;
	}

	public unsafe void Reset(int value = 0)
	{
		*Counter = value;
	}

	public unsafe int Add(int value)
	{
		return Interlocked.Add(ref UnsafeUtility.AsRef<int>(Counter), value) - value;
	}

	public int Sub(int value)
	{
		return Add(-value);
	}

	public unsafe int AddSat(int value, int max = int.MaxValue)
	{
		int num = *Counter;
		int num2;
		do
		{
			num2 = num;
			num = ((num >= max) ? max : math.min(max, num + value));
			num = Interlocked.CompareExchange(ref UnsafeUtility.AsRef<int>(Counter), num, num2);
		}
		while (num2 != num && num2 != max);
		return num2;
	}

	public unsafe int SubSat(int value, int min = int.MinValue)
	{
		int num = *Counter;
		int num2;
		do
		{
			num2 = num;
			num = ((num <= min) ? min : math.max(min, num - value));
			num = Interlocked.CompareExchange(ref UnsafeUtility.AsRef<int>(Counter), num, num2);
		}
		while (num2 != num && num2 != min);
		return num2;
	}
}
