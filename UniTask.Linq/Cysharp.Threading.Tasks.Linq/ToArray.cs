using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using Cysharp.Threading.Tasks.Internal;

namespace Cysharp.Threading.Tasks.Linq;

internal static class ToArray
{
	internal static async UniTask<TSource[]> ToArrayAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
	{
		ArrayPool<TSource> pool = ArrayPool<TSource>.Shared;
		TSource[] array = pool.Rent(16);
		TSource[] result = null;
		IUniTaskAsyncEnumerator<TSource> e = null;
		object obj = null;
		try
		{
			e = source.GetAsyncEnumerator(cancellationToken);
			int i = 0;
			while (await e.MoveNextAsync())
			{
				ArrayPoolUtil.EnsureCapacity(ref array, i, pool);
				array[i++] = e.Current;
			}
			if (i == 0)
			{
				result = Array.Empty<TSource>();
			}
			else
			{
				result = new TSource[i];
				Array.Copy(array, result, i);
			}
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		pool.Return(array, !RuntimeHelpersAbstraction.IsWellKnownNoReferenceContainsType<TSource>());
		if (e != null)
		{
			await e.DisposeAsync();
		}
		object obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
		return result;
	}
}
