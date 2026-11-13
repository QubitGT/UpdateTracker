using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Cysharp.Threading.Tasks.Linq;

internal static class Any
{
	internal static async UniTask<bool> AnyAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		bool result = default(bool);
		try
		{
			result = ((await e.MoveNextAsync()) ? true : false);
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

	internal static async UniTask<bool> AnyAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
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
					if (predicate(e.Current))
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

	internal static async UniTask<bool> AnyAwaitAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask<bool>> predicate, CancellationToken cancellationToken)
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
					if (await predicate(e.Current))
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

	internal static async UniTask<bool> AnyAwaitWithCancellationAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask<bool>> predicate, CancellationToken cancellationToken)
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
					if (await predicate(e.Current, cancellationToken))
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
