using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using Cysharp.Threading.Tasks.Internal;

namespace Cysharp.Threading.Tasks.Linq;

internal static class SingleOperator
{
	public static async UniTask<TSource> SingleAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken, bool defaultIfEmpty)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		TSource result = default(TSource);
		try
		{
			if (await e.MoveNextAsync())
			{
				TSource v = e.Current;
				if (await e.MoveNextAsync())
				{
					throw Error.MoreThanOneElement();
				}
				result = v;
			}
			else
			{
				if (!defaultIfEmpty)
				{
					throw Error.NoElements();
				}
				result = default(TSource);
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

	public static async UniTask<TSource> SingleAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken, bool defaultIfEmpty)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		TSource result = default(TSource);
		try
		{
			TSource value = default(TSource);
			bool found = false;
			while (await e.MoveNextAsync())
			{
				TSource current = e.Current;
				if (predicate(current))
				{
					if (found)
					{
						throw Error.MoreThanOneElement();
					}
					found = true;
					value = current;
				}
			}
			if (!(found || defaultIfEmpty))
			{
				throw Error.NoElements();
			}
			result = value;
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

	public static async UniTask<TSource> SingleAwaitAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask<bool>> predicate, CancellationToken cancellationToken, bool defaultIfEmpty)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		TSource result = default(TSource);
		try
		{
			TSource value = default(TSource);
			bool found = false;
			while (await e.MoveNextAsync())
			{
				TSource v = e.Current;
				if (await predicate(v))
				{
					if (found)
					{
						throw Error.MoreThanOneElement();
					}
					found = true;
					value = v;
				}
			}
			if (!(found || defaultIfEmpty))
			{
				throw Error.NoElements();
			}
			result = value;
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

	public static async UniTask<TSource> SingleAwaitWithCancellationAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask<bool>> predicate, CancellationToken cancellationToken, bool defaultIfEmpty)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		TSource result = default(TSource);
		try
		{
			TSource value = default(TSource);
			bool found = false;
			while (await e.MoveNextAsync())
			{
				TSource v = e.Current;
				if (await predicate(v, cancellationToken))
				{
					if (found)
					{
						throw Error.MoreThanOneElement();
					}
					found = true;
					value = v;
				}
			}
			if (!(found || defaultIfEmpty))
			{
				throw Error.NoElements();
			}
			result = value;
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
