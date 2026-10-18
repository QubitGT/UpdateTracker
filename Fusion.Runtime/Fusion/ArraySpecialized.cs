#define DEBUG
using System.Runtime.CompilerServices;

namespace Fusion;

internal static class ArraySpecialized
{
	private const int IntrosortSizeThreshold = 16;

	private static int FloorLog2(int n)
	{
		int num = 0;
		while (n >= 1)
		{
			num++;
			n /= 2;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Swap<T>(T[] a, int i, int j)
	{
		Assert.Check(i != j);
		T val = a[i];
		a[i] = a[j];
		a[j] = val;
	}

	public static void Sort(int[] array, int index, int length)
	{
		if (length >= 2)
		{
			IntroSort(array, index, length + index - 1, 2 * FloorLog2(array.Length));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int Compare(int x, int y)
	{
		return x - y;
	}

	private static void SwapIfGreater(int[] array, int a, int b)
	{
		if (a != b && Compare(array[a], array[b]) > 0)
		{
			int num = array[a];
			array[a] = array[b];
			array[b] = num;
		}
	}

	private static void IntroSort(int[] array, int lo, int hi, int depthLimit)
	{
		while (hi > lo)
		{
			int num = hi - lo + 1;
			if (num <= 16)
			{
				switch (num)
				{
				case 1:
					break;
				case 2:
					SwapIfGreater(array, lo, hi);
					break;
				case 3:
					SwapIfGreater(array, lo, hi - 1);
					SwapIfGreater(array, lo, hi);
					SwapIfGreater(array, hi - 1, hi);
					break;
				default:
					InsertionSort(array, lo, hi);
					break;
				}
				break;
			}
			if (depthLimit == 0)
			{
				Heapsort(array, lo, hi);
				break;
			}
			depthLimit--;
			int num2 = PickPivotAndPartition(array, lo, hi);
			IntroSort(array, num2 + 1, hi, depthLimit);
			hi = num2 - 1;
		}
	}

	private static int PickPivotAndPartition(int[] array, int lo, int hi)
	{
		int num = lo + (hi - lo) / 2;
		SwapIfGreater(array, lo, num);
		SwapIfGreater(array, lo, hi);
		SwapIfGreater(array, num, hi);
		int num2 = array[num];
		Swap(array, num, hi - 1);
		int num3 = lo;
		int num4 = hi - 1;
		while (num3 < num4)
		{
			while (Compare(array[++num3], num2) < 0)
			{
			}
			while (Compare(num2, array[--num4]) < 0)
			{
			}
			if (num3 >= num4)
			{
				break;
			}
			Swap(array, num3, num4);
		}
		if (num3 != hi - 1)
		{
			Swap(array, num3, hi - 1);
		}
		return num3;
	}

	private static void Heapsort(int[] array, int lo, int hi)
	{
		int num = hi - lo + 1;
		for (int num2 = num / 2; num2 >= 1; num2--)
		{
			DownHeap(array, num2, num, lo);
		}
		for (int num3 = num; num3 > 1; num3--)
		{
			Swap(array, lo, lo + num3 - 1);
			DownHeap(array, 1, num3 - 1, lo);
		}
	}

	private static void DownHeap(int[] array, int i, int n, int lo)
	{
		int num = array[lo + i - 1];
		while (i <= n / 2)
		{
			int num2 = 2 * i;
			if (num2 < n && Compare(array[lo + num2 - 1], array[lo + num2]) < 0)
			{
				num2++;
			}
			if (Compare(num, array[lo + num2 - 1]) >= 0)
			{
				break;
			}
			array[lo + i - 1] = array[lo + num2 - 1];
			i = num2;
		}
		array[lo + i - 1] = num;
	}

	private static void InsertionSort(int[] array, int lo, int hi)
	{
		for (int i = lo; i < hi; i++)
		{
			int num = i;
			int num2 = array[i + 1];
			while (num >= lo && Compare(num2, array[num]) < 0)
			{
				array[num + 1] = array[num];
				num--;
			}
			array[num + 1] = num2;
		}
	}

	public static void Sort(SimulationInput[] array, int index, int length)
	{
		if (length >= 2)
		{
			IntroSort(array, index, length + index - 1, 2 * FloorLog2(array.Length));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static int Compare(SimulationInput x, SimulationInput y)
	{
		return x.Header->Tick.CompareTo(y.Header->Tick);
	}

	private static void SwapIfGreater(SimulationInput[] array, int a, int b)
	{
		if (a != b && Compare(array[a], array[b]) > 0)
		{
			SimulationInput simulationInput = array[a];
			array[a] = array[b];
			array[b] = simulationInput;
		}
	}

	private static void IntroSort(SimulationInput[] array, int lo, int hi, int depthLimit)
	{
		while (hi > lo)
		{
			int num = hi - lo + 1;
			if (num <= 16)
			{
				switch (num)
				{
				case 1:
					break;
				case 2:
					SwapIfGreater(array, lo, hi);
					break;
				case 3:
					SwapIfGreater(array, lo, hi - 1);
					SwapIfGreater(array, lo, hi);
					SwapIfGreater(array, hi - 1, hi);
					break;
				default:
					InsertionSort(array, lo, hi);
					break;
				}
				break;
			}
			if (depthLimit == 0)
			{
				Heapsort(array, lo, hi);
				break;
			}
			depthLimit--;
			int num2 = PickPivotAndPartition(array, lo, hi);
			IntroSort(array, num2 + 1, hi, depthLimit);
			hi = num2 - 1;
		}
	}

	private static int PickPivotAndPartition(SimulationInput[] array, int lo, int hi)
	{
		int num = lo + (hi - lo) / 2;
		SwapIfGreater(array, lo, num);
		SwapIfGreater(array, lo, hi);
		SwapIfGreater(array, num, hi);
		SimulationInput simulationInput = array[num];
		Swap(array, num, hi - 1);
		int num2 = lo;
		int num3 = hi - 1;
		while (num2 < num3)
		{
			while (Compare(array[++num2], simulationInput) < 0)
			{
			}
			while (Compare(simulationInput, array[--num3]) < 0)
			{
			}
			if (num2 >= num3)
			{
				break;
			}
			Swap(array, num2, num3);
		}
		if (num2 != hi - 1)
		{
			Swap(array, num2, hi - 1);
		}
		return num2;
	}

	private static void Heapsort(SimulationInput[] array, int lo, int hi)
	{
		int num = hi - lo + 1;
		for (int num2 = num / 2; num2 >= 1; num2--)
		{
			DownHeap(array, num2, num, lo);
		}
		for (int num3 = num; num3 > 1; num3--)
		{
			Swap(array, lo, lo + num3 - 1);
			DownHeap(array, 1, num3 - 1, lo);
		}
	}

	private static void DownHeap(SimulationInput[] array, int i, int n, int lo)
	{
		SimulationInput simulationInput = array[lo + i - 1];
		while (i <= n / 2)
		{
			int num = 2 * i;
			if (num < n && Compare(array[lo + num - 1], array[lo + num]) < 0)
			{
				num++;
			}
			if (Compare(simulationInput, array[lo + num - 1]) >= 0)
			{
				break;
			}
			array[lo + i - 1] = array[lo + num - 1];
			i = num;
		}
		array[lo + i - 1] = simulationInput;
	}

	private static void InsertionSort(SimulationInput[] array, int lo, int hi)
	{
		for (int i = lo; i < hi; i++)
		{
			int num = i;
			SimulationInput simulationInput = array[i + 1];
			while (num >= lo && Compare(simulationInput, array[num]) < 0)
			{
				array[num + 1] = array[num];
				num--;
			}
			array[num + 1] = simulationInput;
		}
	}
}
