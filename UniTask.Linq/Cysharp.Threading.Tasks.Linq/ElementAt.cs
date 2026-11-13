using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using Cysharp.Threading.Tasks.Internal;

namespace Cysharp.Threading.Tasks.Linq;

internal static class ElementAt
{
	public static async UniTask<TSource> ElementAtAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, int index, CancellationToken cancellationToken, bool defaultIfEmpty)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		TSource result = default(TSource);
		try
		{
			int i = 0;
			while (true)
			{
				if (await e.MoveNextAsync())
				{
					if (i++ == index)
					{
						result = e.Current;
						break;
					}
					continue;
				}
				if (defaultIfEmpty)
				{
					result = default(TSource);
					break;
				}
				throw Error.ArgumentOutOfRange("index");
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
		TSource result2 = default(TSource);
		return result2;
	}
}
