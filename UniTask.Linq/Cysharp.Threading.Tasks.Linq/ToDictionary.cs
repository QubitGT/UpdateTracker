using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Cysharp.Threading.Tasks.Linq;

internal static class ToDictionary
{
	internal static async UniTask<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		Dictionary<TKey, TSource> dict = new Dictionary<TKey, TSource>(comparer);
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		try
		{
			while (await e.MoveNextAsync())
			{
				TSource current = e.Current;
				TKey key = keySelector(current);
				dict.Add(key, current);
			}
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
		return dict;
	}

	internal static async UniTask<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		Dictionary<TKey, TElement> dict = new Dictionary<TKey, TElement>(comparer);
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		try
		{
			while (await e.MoveNextAsync())
			{
				TSource current = e.Current;
				TKey key = keySelector(current);
				TElement value = elementSelector(current);
				dict.Add(key, value);
			}
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
		return dict;
	}

	internal static async UniTask<Dictionary<TKey, TSource>> ToDictionaryAwaitAsync<TSource, TKey>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask<TKey>> keySelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		Dictionary<TKey, TSource> dict = new Dictionary<TKey, TSource>(comparer);
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		try
		{
			while (await e.MoveNextAsync())
			{
				TSource v = e.Current;
				dict.Add(await keySelector(v), v);
			}
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
		return dict;
	}

	internal static async UniTask<Dictionary<TKey, TElement>> ToDictionaryAwaitAsync<TSource, TKey, TElement>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask<TKey>> keySelector, Func<TSource, UniTask<TElement>> elementSelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		Dictionary<TKey, TElement> dict = new Dictionary<TKey, TElement>(comparer);
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		try
		{
			while (await e.MoveNextAsync())
			{
				TSource v = e.Current;
				dict.Add(await keySelector(v), await elementSelector(v));
			}
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
		return dict;
	}

	internal static async UniTask<Dictionary<TKey, TSource>> ToDictionaryAwaitWithCancellationAsync<TSource, TKey>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask<TKey>> keySelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		Dictionary<TKey, TSource> dict = new Dictionary<TKey, TSource>(comparer);
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		try
		{
			while (await e.MoveNextAsync())
			{
				TSource v = e.Current;
				dict.Add(await keySelector(v, cancellationToken), v);
			}
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
		return dict;
	}

	internal static async UniTask<Dictionary<TKey, TElement>> ToDictionaryAwaitWithCancellationAsync<TSource, TKey, TElement>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask<TKey>> keySelector, Func<TSource, CancellationToken, UniTask<TElement>> elementSelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		Dictionary<TKey, TElement> dict = new Dictionary<TKey, TElement>(comparer);
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		try
		{
			while (await e.MoveNextAsync())
			{
				TSource v = e.Current;
				dict.Add(await keySelector(v, cancellationToken), await elementSelector(v, cancellationToken));
			}
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
		return dict;
	}
}
