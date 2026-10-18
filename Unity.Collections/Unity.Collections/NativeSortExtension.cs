using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility]
public static class NativeSortExtension
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public struct DefaultComparer<T> : IComparer<T> where T : IComparable<T>
	{
		public int Compare(T x, T y)
		{
			return x.CompareTo(y);
		}
	}

	private const int k_IntrosortSizeThreshold = 16;

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static void Sort<T>(T* array, int length) where T : unmanaged, IComparable<T>
	{
		IntroSort<T, DefaultComparer<T>>(array, length, default(DefaultComparer<T>));
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	public unsafe static void Sort<T, U>(T* array, int length, U comp) where T : unmanaged where U : IComparer<T>
	{
		IntroSort<T, U>(array, length, comp);
	}

	[GenerateTestsForBurstCompatibility(RequiredUnityDefine = "UNITY_2020_2_OR_NEWER", GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static SortJob<T, DefaultComparer<T>> SortJob<T>(T* array, int length) where T : unmanaged, IComparable<T>
	{
		SortJob<T, DefaultComparer<T>> result = default(SortJob<T, DefaultComparer<T>>);
		result.Data = array;
		result.Length = length;
		result.Comp = default(DefaultComparer<T>);
		return result;
	}

	[GenerateTestsForBurstCompatibility(RequiredUnityDefine = "UNITY_2020_2_OR_NEWER", GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	public unsafe static SortJob<T, U> SortJob<T, U>(T* array, int length, U comp) where T : unmanaged where U : IComparer<T>
	{
		SortJob<T, U> result = default(SortJob<T, U>);
		result.Data = array;
		result.Length = length;
		result.Comp = comp;
		return result;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static int BinarySearch<T>(T* ptr, int length, T value) where T : unmanaged, IComparable<T>
	{
		return BinarySearch(ptr, length, value, default(DefaultComparer<T>));
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	public unsafe static int BinarySearch<T, U>(T* ptr, int length, T value, U comp) where T : unmanaged where U : IComparer<T>
	{
		int num = 0;
		for (int num2 = length; num2 != 0; num2 >>= 1)
		{
			int num3 = num + (num2 >> 1);
			T y = ptr[num3];
			int num4 = comp.Compare(value, y);
			if (num4 == 0)
			{
				return num3;
			}
			if (num4 > 0)
			{
				num = num3 + 1;
				num2--;
			}
		}
		return ~num;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static void Sort<T>(this NativeArray<T> array) where T : unmanaged, IComparable<T>
	{
		IntroSortStruct<T, DefaultComparer<T>>(array.GetUnsafePtr(), array.Length, default(DefaultComparer<T>));
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	public unsafe static void Sort<T, U>(this NativeArray<T> array, U comp) where T : unmanaged where U : IComparer<T>
	{
		T* unsafePtr = (T*)array.GetUnsafePtr();
		int length = array.Length;
		IntroSortStruct<T, U>(unsafePtr, length, comp);
	}

	[GenerateTestsForBurstCompatibility(RequiredUnityDefine = "UNITY_2020_2_OR_NEWER", GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static SortJob<T, DefaultComparer<T>> SortJob<T>(this NativeArray<T> array) where T : unmanaged, IComparable<T>
	{
		return SortJob((T*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(array), array.Length, default(DefaultComparer<T>));
	}

	[GenerateTestsForBurstCompatibility(RequiredUnityDefine = "UNITY_2020_2_OR_NEWER", GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	public unsafe static SortJob<T, U> SortJob<T, U>(this NativeArray<T> array, U comp) where T : unmanaged where U : IComparer<T>
	{
		T* unsafeBufferPointerWithoutChecks = (T*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(array);
		int length = array.Length;
		SortJob<T, U> result = default(SortJob<T, U>);
		result.Data = unsafeBufferPointerWithoutChecks;
		result.Length = length;
		result.Comp = comp;
		return result;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static int BinarySearch<T>(this NativeArray<T> array, T value) where T : unmanaged, IComparable<T>
	{
		return array.BinarySearch(value, default(DefaultComparer<T>));
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	public unsafe static int BinarySearch<T, U>(this NativeArray<T> array, T value, U comp) where T : unmanaged where U : IComparer<T>
	{
		return BinarySearch((T*)array.GetUnsafeReadOnlyPtr(), array.Length, value, comp);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static int BinarySearch<T>(this NativeArray<T>.ReadOnly array, T value) where T : unmanaged, IComparable<T>
	{
		return array.BinarySearch(value, default(DefaultComparer<T>));
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	public unsafe static int BinarySearch<T, U>(this NativeArray<T>.ReadOnly array, T value, U comp) where T : unmanaged where U : IComparer<T>
	{
		return BinarySearch((T*)array.GetUnsafeReadOnlyPtr(), array.Length, value, comp);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static void Sort<T>(this NativeList<T> list) where T : unmanaged, IComparable<T>
	{
		list.Sort(default(DefaultComparer<T>));
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	public unsafe static void Sort<T, U>(this NativeList<T> list, U comp) where T : unmanaged where U : IComparer<T>
	{
		IntroSort<T, U>(list.GetUnsafePtr(), list.Length, comp);
	}

	[GenerateTestsForBurstCompatibility(RequiredUnityDefine = "UNITY_2020_2_OR_NEWER", GenericTypeArguments = new Type[] { typeof(int) })]
	public static SortJob<T, DefaultComparer<T>> SortJob<T>(this NativeList<T> list) where T : unmanaged, IComparable<T>
	{
		return list.SortJob(default(DefaultComparer<T>));
	}

	[GenerateTestsForBurstCompatibility(RequiredUnityDefine = "UNITY_2020_2_OR_NEWER", GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	public unsafe static SortJob<T, U> SortJob<T, U>(this NativeList<T> list, U comp) where T : unmanaged where U : IComparer<T>
	{
		return SortJob(list.GetUnsafePtr(), list.Length, comp);
	}

	[GenerateTestsForBurstCompatibility(RequiredUnityDefine = "UNITY_2020_2_OR_NEWER", GenericTypeArguments = new Type[] { typeof(int) })]
	public static SortJobDefer<T, DefaultComparer<T>> SortJobDefer<T>(this NativeList<T> list) where T : unmanaged, IComparable<T>
	{
		return list.SortJobDefer(default(DefaultComparer<T>));
	}

	[GenerateTestsForBurstCompatibility(RequiredUnityDefine = "UNITY_2020_2_OR_NEWER", GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	public static SortJobDefer<T, U> SortJobDefer<T, U>(this NativeList<T> list, U comp) where T : unmanaged where U : IComparer<T>
	{
		SortJobDefer<T, U> result = default(SortJobDefer<T, U>);
		result.Data = list;
		result.Comp = comp;
		return result;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static int BinarySearch<T>(this NativeList<T> list, T value) where T : unmanaged, IComparable<T>
	{
		return list.AsReadOnly().BinarySearch(value, default(DefaultComparer<T>));
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	public static int BinarySearch<T, U>(this NativeList<T> list, T value, U comp) where T : unmanaged where U : IComparer<T>
	{
		return list.AsReadOnly().BinarySearch(value, comp);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static void Sort<T>(this UnsafeList<T> list) where T : unmanaged, IComparable<T>
	{
		list.Sort(default(DefaultComparer<T>));
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	public unsafe static void Sort<T, U>(this UnsafeList<T> list, U comp) where T : unmanaged where U : IComparer<T>
	{
		IntroSort<T, U>(list.Ptr, list.Length, comp);
	}

	[GenerateTestsForBurstCompatibility(RequiredUnityDefine = "UNITY_2020_2_OR_NEWER", GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static SortJob<T, DefaultComparer<T>> SortJob<T>(this UnsafeList<T> list) where T : unmanaged, IComparable<T>
	{
		return SortJob(list.Ptr, list.Length, default(DefaultComparer<T>));
	}

	[GenerateTestsForBurstCompatibility(RequiredUnityDefine = "UNITY_2020_2_OR_NEWER", GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	public unsafe static SortJob<T, U> SortJob<T, U>(this UnsafeList<T> list, U comp) where T : unmanaged where U : IComparer<T>
	{
		return SortJob(list.Ptr, list.Length, comp);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static int BinarySearch<T>(this UnsafeList<T> list, T value) where T : unmanaged, IComparable<T>
	{
		return list.BinarySearch(value, default(DefaultComparer<T>));
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	public unsafe static int BinarySearch<T, U>(this UnsafeList<T> list, T value, U comp) where T : unmanaged where U : IComparer<T>
	{
		return BinarySearch(list.Ptr, list.Length, value, comp);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static void Sort<T>(this NativeSlice<T> slice) where T : unmanaged, IComparable<T>
	{
		slice.Sort(default(DefaultComparer<T>));
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	public unsafe static void Sort<T, U>(this NativeSlice<T> slice, U comp) where T : unmanaged where U : IComparer<T>
	{
		T* unsafePtr = (T*)slice.GetUnsafePtr();
		int length = slice.Length;
		IntroSortStruct<T, U>(unsafePtr, length, comp);
	}

	[GenerateTestsForBurstCompatibility(RequiredUnityDefine = "UNITY_2020_2_OR_NEWER", GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static SortJob<T, DefaultComparer<T>> SortJob<T>(this NativeSlice<T> slice) where T : unmanaged, IComparable<T>
	{
		return SortJob((T*)slice.GetUnsafePtr(), slice.Length, default(DefaultComparer<T>));
	}

	[GenerateTestsForBurstCompatibility(RequiredUnityDefine = "UNITY_2020_2_OR_NEWER", GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	public unsafe static SortJob<T, U> SortJob<T, U>(this NativeSlice<T> slice, U comp) where T : unmanaged where U : IComparer<T>
	{
		return SortJob((T*)slice.GetUnsafePtr(), slice.Length, comp);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static int BinarySearch<T>(this NativeSlice<T> slice, T value) where T : unmanaged, IComparable<T>
	{
		return slice.BinarySearch(value, default(DefaultComparer<T>));
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	public unsafe static int BinarySearch<T, U>(this NativeSlice<T> slice, T value, U comp) where T : unmanaged where U : IComparer<T>
	{
		return BinarySearch((T*)slice.GetUnsafeReadOnlyPtr(), slice.Length, value, comp);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	internal unsafe static void IntroSort<T, U>(void* array, int length, U comp) where T : unmanaged where U : IComparer<T>
	{
		IntroSort_R<T, U>(array, 0, length - 1, 2 * CollectionHelper.Log2Floor(length), comp);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(DefaultComparer<int>)
	})]
	internal unsafe static void IntroSort_R<T, U>(void* array, int lo, int hi, int depth, U comp) where T : unmanaged where U : IComparer<T>
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
					SwapIfGreaterWithItems<T, U>(array, lo, hi, comp);
					break;
				case 3:
					SwapIfGreaterWithItems<T, U>(array, lo, hi - 1, comp);
					SwapIfGreaterWithItems<T, U>(array, lo, hi, comp);
					SwapIfGreaterWithItems<T, U>(array, hi - 1, hi, comp);
					break;
				default:
					InsertionSort<T, U>(array, lo, hi, comp);
					break;
				}
				break;
			}
			if (depth == 0)
			{
				HeapSort<T, U>(array, lo, hi, comp);
				break;
			}
			depth--;
			int num2 = Partition<T, U>(array, lo, hi, comp);
			IntroSort_R<T, U>(array, num2 + 1, hi, depth, comp);
			hi = num2 - 1;
		}
	}

	private unsafe static void InsertionSort<T, U>(void* array, int lo, int hi, U comp) where T : unmanaged where U : IComparer<T>
	{
		for (int i = lo; i < hi; i++)
		{
			int num = i;
			T val = UnsafeUtility.ReadArrayElement<T>(array, i + 1);
			while (num >= lo && comp.Compare(val, UnsafeUtility.ReadArrayElement<T>(array, num)) < 0)
			{
				UnsafeUtility.WriteArrayElement(array, num + 1, UnsafeUtility.ReadArrayElement<T>(array, num));
				num--;
			}
			UnsafeUtility.WriteArrayElement(array, num + 1, val);
		}
	}

	private unsafe static int Partition<T, U>(void* array, int lo, int hi, U comp) where T : unmanaged where U : IComparer<T>
	{
		int num = lo + (hi - lo) / 2;
		SwapIfGreaterWithItems<T, U>(array, lo, num, comp);
		SwapIfGreaterWithItems<T, U>(array, lo, hi, comp);
		SwapIfGreaterWithItems<T, U>(array, num, hi, comp);
		T x = UnsafeUtility.ReadArrayElement<T>(array, num);
		Swap<T>(array, num, hi - 1);
		int num2 = lo;
		int num3 = hi - 1;
		while (num2 < num3)
		{
			while (num2 < hi && comp.Compare(x, UnsafeUtility.ReadArrayElement<T>(array, ++num2)) > 0)
			{
			}
			while (num3 > num2 && comp.Compare(x, UnsafeUtility.ReadArrayElement<T>(array, --num3)) < 0)
			{
			}
			if (num2 >= num3)
			{
				break;
			}
			Swap<T>(array, num2, num3);
		}
		Swap<T>(array, num2, hi - 1);
		return num2;
	}

	private unsafe static void HeapSort<T, U>(void* array, int lo, int hi, U comp) where T : unmanaged where U : IComparer<T>
	{
		int num = hi - lo + 1;
		for (int num2 = num / 2; num2 >= 1; num2--)
		{
			Heapify<T, U>(array, num2, num, lo, comp);
		}
		for (int num3 = num; num3 > 1; num3--)
		{
			Swap<T>(array, lo, lo + num3 - 1);
			Heapify<T, U>(array, 1, num3 - 1, lo, comp);
		}
	}

	private unsafe static void Heapify<T, U>(void* array, int i, int n, int lo, U comp) where T : unmanaged where U : IComparer<T>
	{
		T val = UnsafeUtility.ReadArrayElement<T>(array, lo + i - 1);
		while (i <= n / 2)
		{
			int num = 2 * i;
			if (num < n && comp.Compare(UnsafeUtility.ReadArrayElement<T>(array, lo + num - 1), UnsafeUtility.ReadArrayElement<T>(array, lo + num)) < 0)
			{
				num++;
			}
			if (comp.Compare(UnsafeUtility.ReadArrayElement<T>(array, lo + num - 1), val) < 0)
			{
				break;
			}
			UnsafeUtility.WriteArrayElement(array, lo + i - 1, UnsafeUtility.ReadArrayElement<T>(array, lo + num - 1));
			i = num;
		}
		UnsafeUtility.WriteArrayElement(array, lo + i - 1, val);
	}

	private unsafe static void Swap<T>(void* array, int lhs, int rhs) where T : unmanaged
	{
		T value = UnsafeUtility.ReadArrayElement<T>(array, lhs);
		UnsafeUtility.WriteArrayElement(array, lhs, UnsafeUtility.ReadArrayElement<T>(array, rhs));
		UnsafeUtility.WriteArrayElement(array, rhs, value);
	}

	private unsafe static void SwapIfGreaterWithItems<T, U>(void* array, int lhs, int rhs, U comp) where T : unmanaged where U : IComparer<T>
	{
		if (lhs != rhs && comp.Compare(UnsafeUtility.ReadArrayElement<T>(array, lhs), UnsafeUtility.ReadArrayElement<T>(array, rhs)) > 0)
		{
			Swap<T>(array, lhs, rhs);
		}
	}

	private unsafe static void IntroSortStruct<T, U>(void* array, int length, U comp) where T : unmanaged where U : IComparer<T>
	{
		int lo = 0;
		int _hi = length - 1;
		IntroSortStruct_R<T, U>(array, in lo, in _hi, 2 * CollectionHelper.Log2Floor(length), comp);
	}

	private unsafe static void IntroSortStruct_R<T, U>(void* array, in int lo, in int _hi, int depth, U comp) where T : unmanaged where U : IComparer<T>
	{
		int hi = _hi;
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
					SwapIfGreaterWithItemsStruct<T, U>(array, lo, hi, comp);
					break;
				case 3:
					SwapIfGreaterWithItemsStruct<T, U>(array, lo, hi - 1, comp);
					SwapIfGreaterWithItemsStruct<T, U>(array, lo, hi, comp);
					SwapIfGreaterWithItemsStruct<T, U>(array, hi - 1, hi, comp);
					break;
				default:
					InsertionSortStruct<T, U>(array, in lo, in hi, comp);
					break;
				}
				break;
			}
			if (depth == 0)
			{
				HeapSortStruct<T, U>(array, in lo, in hi, comp);
				break;
			}
			depth--;
			int num2 = PartitionStruct<T, U>(array, in lo, in hi, comp);
			int lo2 = num2 + 1;
			IntroSortStruct_R<T, U>(array, in lo2, in hi, depth, comp);
			hi = num2 - 1;
		}
	}

	private unsafe static void InsertionSortStruct<T, U>(void* array, in int lo, in int hi, U comp) where T : unmanaged where U : IComparer<T>
	{
		for (int i = lo; i < hi; i++)
		{
			int num = i;
			T val = UnsafeUtility.ReadArrayElement<T>(array, i + 1);
			while (num >= lo && comp.Compare(val, UnsafeUtility.ReadArrayElement<T>(array, num)) < 0)
			{
				UnsafeUtility.WriteArrayElement(array, num + 1, UnsafeUtility.ReadArrayElement<T>(array, num));
				num--;
			}
			UnsafeUtility.WriteArrayElement(array, num + 1, val);
		}
	}

	private unsafe static int PartitionStruct<T, U>(void* array, in int lo, in int hi, U comp) where T : unmanaged where U : IComparer<T>
	{
		int num = lo + (hi - lo) / 2;
		SwapIfGreaterWithItemsStruct<T, U>(array, lo, num, comp);
		SwapIfGreaterWithItemsStruct<T, U>(array, lo, hi, comp);
		SwapIfGreaterWithItemsStruct<T, U>(array, num, hi, comp);
		T x = UnsafeUtility.ReadArrayElement<T>(array, num);
		SwapStruct<T>(array, num, hi - 1);
		int num2 = lo;
		int num3 = hi - 1;
		while (num2 < num3)
		{
			while (num2 < hi && comp.Compare(x, UnsafeUtility.ReadArrayElement<T>(array, ++num2)) > 0)
			{
			}
			while (num3 > num2 && comp.Compare(x, UnsafeUtility.ReadArrayElement<T>(array, --num3)) < 0)
			{
			}
			if (num2 >= num3)
			{
				break;
			}
			SwapStruct<T>(array, num2, num3);
		}
		SwapStruct<T>(array, num2, hi - 1);
		return num2;
	}

	private unsafe static void HeapSortStruct<T, U>(void* array, in int lo, in int hi, U comp) where T : unmanaged where U : IComparer<T>
	{
		int num = hi - lo + 1;
		for (int num2 = num / 2; num2 >= 1; num2--)
		{
			HeapifyStruct<T, U>(array, num2, num, in lo, comp);
		}
		for (int num3 = num; num3 > 1; num3--)
		{
			SwapStruct<T>(array, lo, lo + num3 - 1);
			HeapifyStruct<T, U>(array, 1, num3 - 1, in lo, comp);
		}
	}

	private unsafe static void HeapifyStruct<T, U>(void* array, int i, int n, in int lo, U comp) where T : unmanaged where U : IComparer<T>
	{
		T val = UnsafeUtility.ReadArrayElement<T>(array, lo + i - 1);
		while (i <= n / 2)
		{
			int num = 2 * i;
			if (num < n && comp.Compare(UnsafeUtility.ReadArrayElement<T>(array, lo + num - 1), UnsafeUtility.ReadArrayElement<T>(array, lo + num)) < 0)
			{
				num++;
			}
			if (comp.Compare(UnsafeUtility.ReadArrayElement<T>(array, lo + num - 1), val) < 0)
			{
				break;
			}
			UnsafeUtility.WriteArrayElement(array, lo + i - 1, UnsafeUtility.ReadArrayElement<T>(array, lo + num - 1));
			i = num;
		}
		UnsafeUtility.WriteArrayElement(array, lo + i - 1, val);
	}

	private unsafe static void SwapStruct<T>(void* array, int lhs, int rhs) where T : unmanaged
	{
		T value = UnsafeUtility.ReadArrayElement<T>(array, lhs);
		UnsafeUtility.WriteArrayElement(array, lhs, UnsafeUtility.ReadArrayElement<T>(array, rhs));
		UnsafeUtility.WriteArrayElement(array, rhs, value);
	}

	private unsafe static void SwapIfGreaterWithItemsStruct<T, U>(void* array, int lhs, int rhs, U comp) where T : unmanaged where U : IComparer<T>
	{
		if (lhs != rhs && comp.Compare(UnsafeUtility.ReadArrayElement<T>(array, lhs), UnsafeUtility.ReadArrayElement<T>(array, rhs)) > 0)
		{
			SwapStruct<T>(array, lhs, rhs);
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckStrideMatchesSize<T>(int stride) where T : unmanaged
	{
		if (stride != UnsafeUtility.SizeOf<T>())
		{
			throw new InvalidOperationException("Sort requires that stride matches the size of the source type");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private unsafe static void CheckComparer<T, U>(T* array, int length, U comp) where T : unmanaged where U : IComparer<T>
	{
		if (length <= 0)
		{
			return;
		}
		T val = *array;
		if (comp.Compare(val, val) != 0)
		{
			throw new InvalidOperationException("Comparison function is incorrect. Compare(a, a) must return 0/equal.");
		}
		int i = 1;
		for (int num = math.min(length, 8); i < num; i++)
		{
			T val2 = array[i];
			if (comp.Compare(val, val2) != 0 || comp.Compare(val2, val) != 0)
			{
				if (comp.Compare(val, val2) == 0)
				{
					throw new InvalidOperationException("Comparison function is incorrect. Compare(a, b) of two different values should not return 0/equal.");
				}
				if (comp.Compare(val2, val) == 0)
				{
					throw new InvalidOperationException("Comparison function is incorrect. Compare(b, a) of two different values should not return 0/equal.");
				}
				if (comp.Compare(val, val2) == comp.Compare(val2, val))
				{
					throw new InvalidOperationException("Comparison function is incorrect. Compare(a, b) when a and b are different values should not return the same value as Compare(b, a).");
				}
				break;
			}
		}
	}
}
