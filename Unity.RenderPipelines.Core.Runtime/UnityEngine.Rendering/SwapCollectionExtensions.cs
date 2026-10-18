using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace UnityEngine.Rendering;

public static class SwapCollectionExtensions
{
	[CollectionAccess(CollectionAccessType.ModifyExistingContent)]
	[MustUseReturnValue]
	public static bool TrySwap<TValue>([DisallowNull] this IList<TValue> list, int from, int to, [NotNullWhen(false)] out Exception error)
	{
		error = null;
		if (list == null)
		{
			error = new ArgumentNullException("list");
		}
		else
		{
			if (from < 0 || from >= list.Count)
			{
				error = new ArgumentOutOfRangeException("from");
			}
			if (to < 0 || to >= list.Count)
			{
				error = new ArgumentOutOfRangeException("to");
			}
		}
		if (error != null)
		{
			return false;
		}
		TValue val = list[from];
		TValue val2 = list[to];
		TValue val4 = (list[to] = val);
		val4 = (list[from] = val2);
		return true;
	}
}
