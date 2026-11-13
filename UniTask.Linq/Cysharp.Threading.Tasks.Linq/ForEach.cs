using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Cysharp.Threading.Tasks.Linq;

internal static class ForEach
{
	public static async UniTask ForEachAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Action<TSource> action, CancellationToken cancellationToken)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		try
		{
			while (await e.MoveNextAsync())
			{
				action(e.Current);
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
	}

	public static async UniTask ForEachAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Action<TSource, int> action, CancellationToken cancellationToken)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		try
		{
			int index = 0;
			while (await e.MoveNextAsync())
			{
				action(e.Current, checked(index++));
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
	}

	public static async UniTask ForEachAwaitAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask> action, CancellationToken cancellationToken)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		try
		{
			while (await e.MoveNextAsync())
			{
				await action(e.Current);
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
	}

	public static async UniTask ForEachAwaitAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, int, UniTask> action, CancellationToken cancellationToken)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		try
		{
			int index = 0;
			while (await e.MoveNextAsync())
			{
				await action(e.Current, checked(index++));
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
	}

	public static async UniTask ForEachAwaitWithCancellationAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask> action, CancellationToken cancellationToken)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		try
		{
			while (await e.MoveNextAsync())
			{
				await action(e.Current, cancellationToken);
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
	}

	public static async UniTask ForEachAwaitWithCancellationAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, UniTask> action, CancellationToken cancellationToken)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		try
		{
			int index = 0;
			while (await e.MoveNextAsync())
			{
				await action(e.Current, checked(index++), cancellationToken);
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
	}
}
