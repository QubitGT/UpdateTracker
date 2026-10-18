using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine.ProBuilder;

internal static class ArrayUtility
{
	private struct SearchRange
	{
		public int begin;

		public int end;

		public SearchRange(int begin, int end)
		{
			this.begin = begin;
			this.end = end;
		}

		public bool Valid()
		{
			return end - begin > 1;
		}

		public int Center()
		{
			return begin + (end - begin) / 2;
		}

		public override string ToString()
		{
			return "{" + begin + ", " + end + "} : " + Center();
		}
	}

	public static T[] ValuesWithIndexes<T>(this T[] arr, int[] indexes)
	{
		T[] array = new T[indexes.Length];
		for (int i = 0; i < indexes.Length; i++)
		{
			array[i] = arr[indexes[i]];
		}
		return array;
	}

	public static List<T> ValuesWithIndexes<T>(this List<T> arr, IList<int> indexes)
	{
		List<T> list = new List<T>(indexes.Count);
		foreach (int index in indexes)
		{
			list.Add(arr[index]);
		}
		return list;
	}

	public static IEnumerable<int> AllIndexesOf<T>(this IList<T> list, Func<T, bool> lambda)
	{
		List<int> list2 = new List<int>();
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (lambda(list[i]))
			{
				list2.Add(i);
			}
		}
		return list2;
	}

	public static T[] Add<T>(this T[] arr, T val)
	{
		T[] array = new T[arr.Length + 1];
		Array.ConstrainedCopy(arr, 0, array, 0, arr.Length);
		array[arr.Length] = val;
		return array;
	}

	public static T[] AddRange<T>(this T[] arr, T[] val)
	{
		T[] array = new T[arr.Length + val.Length];
		Array.ConstrainedCopy(arr, 0, array, 0, arr.Length);
		Array.ConstrainedCopy(val, 0, array, arr.Length, val.Length);
		return array;
	}

	public static T[] Remove<T>(this T[] arr, T val)
	{
		List<T> list = new List<T>(arr);
		list.Remove(val);
		return list.ToArray();
	}

	public static T[] Remove<T>(this T[] arr, IEnumerable<T> val)
	{
		return arr.Except(val).ToArray();
	}

	public static T[] RemoveAt<T>(this T[] arr, int index)
	{
		T[] array = new T[arr.Length - 1];
		int num = 0;
		for (int i = 0; i < arr.Length; i++)
		{
			if (i != index)
			{
				array[num] = arr[i];
				num++;
			}
		}
		return array;
	}

	public static T[] RemoveAt<T>(this IList<T> list, IEnumerable<int> indexes)
	{
		List<int> list2 = new List<int>(indexes);
		list2.Sort();
		return list.SortedRemoveAt(list2);
	}

	public static T[] SortedRemoveAt<T>(this IList<T> list, IList<int> sorted)
	{
		int count = sorted.Count;
		int count2 = list.Count;
		T[] array = new T[count2 - count];
		int i = 0;
		for (int j = 0; j < count2; j++)
		{
			if (i < count && sorted[i] == j)
			{
				for (; i < count && sorted[i] == j; i++)
				{
				}
			}
			else
			{
				array[j - i] = list[j];
			}
		}
		return array;
	}

	public static int NearestIndexPriorToValue<T>(IList<T> sorted_list, T value) where T : IComparable<T>
	{
		int count = sorted_list.Count;
		if (count < 1)
		{
			return -1;
		}
		SearchRange searchRange = new SearchRange(0, count - 1);
		if (value.CompareTo(sorted_list[0]) < 0)
		{
			return -1;
		}
		if (value.CompareTo(sorted_list[count - 1]) > 0)
		{
			return count - 1;
		}
		while (searchRange.Valid())
		{
			if (sorted_list[searchRange.Center()].CompareTo(value) > 0)
			{
				searchRange.end = searchRange.Center();
				continue;
			}
			searchRange.begin = searchRange.Center();
			if (sorted_list[searchRange.begin + 1].CompareTo(value) < 0)
			{
				continue;
			}
			return searchRange.begin;
		}
		return 0;
	}

	public static List<T> Fill<T>(Func<int, T> ctor, int length)
	{
		List<T> list = new List<T>(length);
		for (int i = 0; i < length; i++)
		{
			list.Add(ctor(i));
		}
		return list;
	}

	public static T[] Fill<T>(T val, int length)
	{
		T[] array = new T[length];
		for (int i = 0; i < length; i++)
		{
			array[i] = val;
		}
		return array;
	}

	public static bool ContainsMatch<T>(this T[] a, T[] b)
	{
		for (int i = 0; i < a.Length; i++)
		{
			if (Array.IndexOf(b, a[i]) > -1)
			{
				return true;
			}
		}
		return false;
	}

	public static bool ContainsMatch<T>(this T[] a, T[] b, out int index_a, out int index_b)
	{
		index_b = -1;
		for (index_a = 0; index_a < a.Length; index_a++)
		{
			index_b = Array.IndexOf(b, a[index_a]);
			if (index_b > -1)
			{
				return true;
			}
		}
		return false;
	}

	public static T[] Concat<T>(this T[] x, T[] y)
	{
		if (x == null)
		{
			throw new ArgumentNullException("x");
		}
		if (y == null)
		{
			throw new ArgumentNullException("y");
		}
		int destinationIndex = x.Length;
		Array.Resize(ref x, x.Length + y.Length);
		Array.Copy(y, 0, x, destinationIndex, y.Length);
		return x;
	}

	public static int IndexOf<T>(this List<List<T>> InList, T InValue)
	{
		for (int i = 0; i < InList.Count; i++)
		{
			for (int j = 0; j < InList[i].Count; j++)
			{
				if (InList[i][j].Equals(InValue))
				{
					return i;
				}
			}
		}
		return -1;
	}

	public static T[] Fill<T>(int count, Func<int, T> ctor)
	{
		T[] array = new T[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = ctor(i);
		}
		return array;
	}

	public static void AddOrAppend<T, K>(this Dictionary<T, List<K>> dictionary, T key, K value)
	{
		if (dictionary.TryGetValue(key, out var value2))
		{
			value2.Add(value);
			return;
		}
		dictionary.Add(key, new List<K> { value });
	}

	public static void AddOrAppendRange<T, K>(this Dictionary<T, List<K>> dictionary, T key, List<K> value)
	{
		if (dictionary.TryGetValue(key, out var value2))
		{
			value2.AddRange(value);
		}
		else
		{
			dictionary.Add(key, value);
		}
	}

	public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		HashSet<TKey> knownKeys = new HashSet<TKey>();
		return source.Where((TSource x) => knownKeys.Add(keySelector(x)));
	}

	public static string ToString<TKey, TValue>(this Dictionary<TKey, TValue> dict)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<TKey, TValue> item in dict)
		{
			stringBuilder.AppendLine($"Key: {item.Key}  Value: {item.Value}");
		}
		return stringBuilder.ToString();
	}

	public static string ToString<T>(this IEnumerable<T> arr, string separator = ", ")
	{
		return string.Join(separator, arr.Select((T x) => x.ToString()).ToArray());
	}
}
