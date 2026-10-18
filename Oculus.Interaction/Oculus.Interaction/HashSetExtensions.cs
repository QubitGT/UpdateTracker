using System;
using System.Collections.Generic;

namespace Oculus.Interaction;

public static class HashSetExtensions
{
	public static void UnionWithNonAlloc<T>(this HashSet<T> hashSetToModify, HashSet<T> other)
	{
		if (hashSetToModify == null)
		{
			throw new ArgumentNullException("hashSetToModify");
		}
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		foreach (T item in other)
		{
			hashSetToModify.Add(item);
		}
	}

	public static void UnionWithNonAlloc<T>(this HashSet<T> hashSetToModify, IList<T> other)
	{
		if (hashSetToModify == null)
		{
			throw new ArgumentNullException("hashSetToModify");
		}
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		for (int i = 0; i < other.Count; i++)
		{
			hashSetToModify.Add(other[i]);
		}
	}

	public static void ExceptWithNonAlloc<T>(this HashSet<T> hashSetToModify, HashSet<T> other)
	{
		if (hashSetToModify == null)
		{
			throw new ArgumentNullException("hashSetToModify");
		}
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (hashSetToModify.Count == 0)
		{
			return;
		}
		if (other == hashSetToModify)
		{
			hashSetToModify.Clear();
			return;
		}
		foreach (T item in other)
		{
			hashSetToModify.Remove(item);
		}
	}

	public static void ExceptWithNonAlloc<T>(this HashSet<T> hashSetToModify, IList<T> other)
	{
		if (hashSetToModify == null)
		{
			throw new ArgumentNullException("hashSetToModify");
		}
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (hashSetToModify.Count != 0)
		{
			for (int i = 0; i < other.Count; i++)
			{
				hashSetToModify.Remove(other[i]);
			}
		}
	}

	public static bool OverlapsNonAlloc<T>(this HashSet<T> hashSetToCheck, HashSet<T> other)
	{
		if (hashSetToCheck == null)
		{
			throw new ArgumentNullException("hashSetToCheck");
		}
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (hashSetToCheck.Count == 0)
		{
			return false;
		}
		foreach (T item in other)
		{
			if (hashSetToCheck.Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool OverlapsNonAlloc<T>(this HashSet<T> hashSetToCheck, IList<T> other)
	{
		if (hashSetToCheck == null)
		{
			throw new ArgumentNullException("hashSetToCheck");
		}
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (hashSetToCheck.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < other.Count; i++)
		{
			if (hashSetToCheck.Contains(other[i]))
			{
				return true;
			}
		}
		return false;
	}
}
