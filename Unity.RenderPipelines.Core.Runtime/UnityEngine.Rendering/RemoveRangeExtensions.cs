using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace UnityEngine.Rendering;

public static class RemoveRangeExtensions
{
	[CollectionAccess(CollectionAccessType.ModifyExistingContent)]
	[MustUseReturnValue]
	public static bool TryRemoveElementsInRange<TValue>([DisallowNull] this IList<TValue> list, int index, int count, [NotNullWhen(false)] out Exception error)
	{
		try
		{
			if (list is List<TValue> list2)
			{
				list2.RemoveRange(index, count);
			}
			else
			{
				for (int num = count; num > 0; num--)
				{
					list.RemoveAt(index);
				}
			}
		}
		catch (Exception ex)
		{
			error = ex;
			return false;
		}
		error = null;
		return true;
	}
}
