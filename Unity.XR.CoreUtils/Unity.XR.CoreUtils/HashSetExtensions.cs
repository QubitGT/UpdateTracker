using System.Collections.Generic;

namespace Unity.XR.CoreUtils;

public static class HashSetExtensions
{
	public static void ExceptWithNonAlloc<T>(this HashSet<T> self, HashSet<T> other)
	{
		foreach (T item in other)
		{
			self.Remove(item);
		}
	}

	public static T First<T>(this HashSet<T> set)
	{
		HashSet<T>.Enumerator enumerator = set.GetEnumerator();
		T result = (enumerator.MoveNext() ? enumerator.Current : default(T));
		enumerator.Dispose();
		return result;
	}
}
