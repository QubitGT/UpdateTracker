using System.Collections.Generic;
using System.Linq;

namespace g3;

public static class DeepCopy
{
	public static List<T> List<T>(IEnumerable<T> Input) where T : IDuplicatable<T>
	{
		List<T> list = new List<T>();
		foreach (T item in Input)
		{
			list.Add(item.Duplicate());
		}
		return list;
	}

	public static T[] Array<T>(IEnumerable<T> Input) where T : IDuplicatable<T>
	{
		T[] array = new T[Input.Count()];
		int num = 0;
		foreach (T item in Input)
		{
			array[num++] = item.Duplicate();
		}
		return array;
	}
}
