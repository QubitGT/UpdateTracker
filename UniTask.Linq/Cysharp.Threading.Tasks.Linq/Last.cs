using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using Cysharp.Threading.Tasks.Internal;

namespace Cysharp.Threading.Tasks.Linq;

internal static class Last
{
	public static async UniTask<TSource> LastAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken, bool defaultIfEmpty)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		TSource result = default(TSource);
		try
		{
			TSource value = default(TSource);
			if (await e.MoveNextAsync())
			{
				value = e.Current;
				while (await e.MoveNextAsync())
				{
					value = e.Current;
				}
				result = value;
			}
			else
			{
				if (!defaultIfEmpty)
				{
					throw Error.NoElements();
				}
				result = value;
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

	public static async UniTask<TSource> LastAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken, bool defaultIfEmpty)
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
					found = true;
					value = current;
				}
			}
			if (defaultIfEmpty)
			{
				result = value;
			}
			else
			{
				if (!found)
				{
					throw Error.NoElements();
				}
				result = value;
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

	public static async UniTask<TSource> LastAwaitAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask<bool>> predicate, CancellationToken cancellationToken, bool defaultIfEmpty)
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
					found = true;
					value = v;
				}
			}
			if (defaultIfEmpty)
			{
				result = value;
			}
			else
			{
				if (!found)
				{
					throw Error.NoElements();
				}
				result = value;
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

	public static async UniTask<TSource> LastAwaitWithCancellationAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask<bool>> predicate, CancellationToken cancellationToken, bool defaultIfEmpty)
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
					found = true;
					value = v;
				}
			}
			if (defaultIfEmpty)
			{
				result = value;
			}
			else
			{
				if (!found)
				{
					throw Error.NoElements();
				}
				result = value;
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
