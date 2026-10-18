using System;
using System.Collections.Generic;

namespace Unity.Collections;

public static class ListExtensions
{
	public static bool RemoveSwapBack<T>(this List<T> list, T value)
	{
		int num = list.IndexOf(value);
		if (num < 0)
		{
			return false;
		}
		list.RemoveAtSwapBack(num);
		return true;
	}

	public static bool RemoveSwapBack<T>(this List<T> list, Predicate<T> matcher)
	{
		int num = list.FindIndex(matcher);
		if (num < 0)
		{
			return false;
		}
		list.RemoveAtSwapBack(num);
		return true;
	}

	public static void RemoveAtSwapBack<T>(this List<T> list, int index)
	{
		int index2 = list.Count - 1;
		list[index] = list[index2];
		list.RemoveAt(index2);
	}

	public static NativeList<T> ToNativeList<T>(this List<T> list, AllocatorManager.AllocatorHandle allocator) where T : unmanaged
	{
		NativeList<T> result = new NativeList<T>(list.Count, allocator);
		for (int i = 0; i < list.Count; i++)
		{
			result.AddNoResize(list[i]);
		}
		return result;
	}

	public static NativeArray<T> ToNativeArray<T>(this List<T> list, AllocatorManager.AllocatorHandle allocator) where T : unmanaged
	{
		NativeArray<T> result = CollectionHelper.CreateNativeArray<T>(list.Count, allocator, NativeArrayOptions.UninitializedMemory);
		for (int i = 0; i < list.Count; i++)
		{
			result[i] = list[i];
		}
		return result;
	}
}
