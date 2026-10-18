using System.Collections.Generic;
using System.Linq;

namespace Meta.WitAi;

internal static class EnumerableExtensions
{
	internal static bool Equivalent<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
	{
		if (first == null && second == null)
		{
			return true;
		}
		if (first == null || second == null)
		{
			return false;
		}
		return first.SequenceEqual(second);
	}
}
