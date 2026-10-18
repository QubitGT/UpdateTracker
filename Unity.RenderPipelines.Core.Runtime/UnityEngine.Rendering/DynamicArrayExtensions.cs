using System;

namespace UnityEngine.Rendering;

public static class DynamicArrayExtensions
{
	private static int Partition<T>(Span<T> data, int left, int right) where T : IComparable<T>, new()
	{
		T other = data[left];
		left--;
		right++;
		while (true)
		{
			int num = 0;
			T val = default(T);
			do
			{
				left++;
				val = data[left];
				num = val.CompareTo(other);
			}
			while (num < 0);
			T val2 = default(T);
			do
			{
				right--;
				val2 = data[right];
				num = val2.CompareTo(other);
			}
			while (num > 0);
			if (left >= right)
			{
				break;
			}
			data[right] = val;
			data[left] = val2;
		}
		return right;
	}

	private static void QuickSort<T>(Span<T> data, int left, int right) where T : IComparable<T>, new()
	{
		if (left < right)
		{
			int num = Partition(data, left, right);
			if (num >= 1)
			{
				QuickSort(data, left, num);
			}
			if (num + 1 < right)
			{
				QuickSort(data, num + 1, right);
			}
		}
	}

	private static int Partition<T>(Span<T> data, int left, int right, DynamicArray<T>.SortComparer comparer) where T : new()
	{
		T y = data[left];
		left--;
		right++;
		while (true)
		{
			int num = 0;
			T val = default(T);
			do
			{
				left++;
				val = data[left];
				num = comparer(val, y);
			}
			while (num < 0);
			T val2 = default(T);
			do
			{
				right--;
				val2 = data[right];
				num = comparer(val2, y);
			}
			while (num > 0);
			if (left >= right)
			{
				break;
			}
			data[right] = val;
			data[left] = val2;
		}
		return right;
	}

	private static void QuickSort<T>(Span<T> data, int left, int right, DynamicArray<T>.SortComparer comparer) where T : new()
	{
		if (left < right)
		{
			int num = Partition(data, left, right, comparer);
			if (num >= 1)
			{
				QuickSort(data, left, num, comparer);
			}
			if (num + 1 < right)
			{
				QuickSort(data, num + 1, right, comparer);
			}
		}
	}

	public static void QuickSort<T>(this DynamicArray<T> array) where T : IComparable<T>, new()
	{
		QuickSort<T>(array, 0, array.size - 1);
		array.BumpVersion();
	}

	public static void QuickSort<T>(this DynamicArray<T> array, DynamicArray<T>.SortComparer comparer) where T : new()
	{
		QuickSort(array, 0, array.size - 1, comparer);
		array.BumpVersion();
	}
}
