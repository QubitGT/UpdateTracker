using System.Collections.Generic;

namespace Unity.XR.CoreUtils;

public static class ListExtensions
{
	public static List<T> Fill<T>(this List<T> list, int count) where T : new()
	{
		for (int i = 0; i < count; i++)
		{
			list.Add(new T());
		}
		return list;
	}

	public static void EnsureCapacity<T>(this List<T> list, int capacity)
	{
		if (list.Capacity < capacity)
		{
			list.Capacity = capacity;
		}
	}

	public static void SwapAtIndices<T>(this List<T> list, int first, int second)
	{
		T val = list[second];
		T val2 = list[first];
		T val4 = (list[first] = val);
		val4 = (list[second] = val2);
	}
}
