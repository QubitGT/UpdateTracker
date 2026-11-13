using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Cysharp.Threading.Tasks.Linq;

internal static class Contains
{
	internal static async UniTask<bool> ContainsAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer, CancellationToken cancellationToken)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		bool result = default(bool);
		try
		{
			while (true)
			{
				if (await e.MoveNextAsync())
				{
					if (comparer.Equals(value, e.Current))
					{
						result = true;
						break;
					}
					continue;
				}
				result = false;
				break;
			}
			num = 1;
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		if (e != null)
		{
			await e.DisposeAsync();
		}
		object obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
		if (num == 1)
		{
			return result;
		}
		bool result2 = default(bool);
		return result2;
	}
}
