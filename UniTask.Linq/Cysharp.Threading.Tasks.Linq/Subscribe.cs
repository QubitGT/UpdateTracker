using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Cysharp.Threading.Tasks.Linq;

internal static class Subscribe
{
	public static readonly Action<Exception> NopError = delegate
	{
	};

	public static readonly Action NopCompleted = delegate
	{
	};

	public static async UniTaskVoid SubscribeCore<TSource>(IUniTaskAsyncEnumerable<TSource> source, Action<TSource> onNext, Action<Exception> onError, Action onCompleted, CancellationToken cancellationToken)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		try
		{
			try
			{
				while (await e.MoveNextAsync())
				{
					try
					{
						onNext(e.Current);
					}
					catch (Exception ex)
					{
						UniTaskScheduler.PublishUnobservedTaskException(ex);
					}
				}
				onCompleted();
			}
			catch (Exception ex2)
			{
				if (onError == NopError)
				{
					UniTaskScheduler.PublishUnobservedTaskException(ex2);
					goto IL_0109;
				}
				if (ex2 is OperationCanceledException)
				{
					goto IL_0109;
				}
				onError(ex2);
			}
			goto end_IL_0037;
			IL_0109:
			num = 1;
			end_IL_0037:;
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
		if (num != 1)
		{
		}
	}

	public static async UniTaskVoid SubscribeCore<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTaskVoid> onNext, Action<Exception> onError, Action onCompleted, CancellationToken cancellationToken)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		try
		{
			try
			{
				while (await e.MoveNextAsync())
				{
					try
					{
						onNext(e.Current).Forget();
					}
					catch (Exception ex)
					{
						UniTaskScheduler.PublishUnobservedTaskException(ex);
					}
				}
				onCompleted();
			}
			catch (Exception ex2)
			{
				if (onError == NopError)
				{
					UniTaskScheduler.PublishUnobservedTaskException(ex2);
					goto IL_0115;
				}
				if (ex2 is OperationCanceledException)
				{
					goto IL_0115;
				}
				onError(ex2);
			}
			goto end_IL_0037;
			IL_0115:
			num = 1;
			end_IL_0037:;
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
		if (num != 1)
		{
		}
	}

	public static async UniTaskVoid SubscribeCore<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTaskVoid> onNext, Action<Exception> onError, Action onCompleted, CancellationToken cancellationToken)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		try
		{
			try
			{
				while (await e.MoveNextAsync())
				{
					try
					{
						onNext(e.Current, cancellationToken).Forget();
					}
					catch (Exception ex)
					{
						UniTaskScheduler.PublishUnobservedTaskException(ex);
					}
				}
				onCompleted();
			}
			catch (Exception ex2)
			{
				if (onError == NopError)
				{
					UniTaskScheduler.PublishUnobservedTaskException(ex2);
					goto IL_011b;
				}
				if (ex2 is OperationCanceledException)
				{
					goto IL_011b;
				}
				onError(ex2);
			}
			goto end_IL_0037;
			IL_011b:
			num = 1;
			end_IL_0037:;
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
		if (num != 1)
		{
		}
	}

	public static async UniTaskVoid SubscribeCore<TSource>(IUniTaskAsyncEnumerable<TSource> source, IObserver<TSource> observer, CancellationToken cancellationToken)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		try
		{
			try
			{
				while (await e.MoveNextAsync())
				{
					try
					{
						observer.OnNext(e.Current);
					}
					catch (Exception ex)
					{
						UniTaskScheduler.PublishUnobservedTaskException(ex);
					}
				}
				observer.OnCompleted();
			}
			catch (Exception ex2)
			{
				if (ex2 is OperationCanceledException)
				{
					goto IL_00ef;
				}
				observer.OnError(ex2);
			}
			goto end_IL_0037;
			IL_00ef:
			num = 1;
			end_IL_0037:;
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
		if (num != 1)
		{
		}
	}

	public static async UniTaskVoid SubscribeAwaitCore<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask> onNext, Action<Exception> onError, Action onCompleted, CancellationToken cancellationToken)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		try
		{
			_ = 1;
			try
			{
				while (await e.MoveNextAsync())
				{
					try
					{
						await onNext(e.Current);
					}
					catch (Exception ex)
					{
						UniTaskScheduler.PublishUnobservedTaskException(ex);
					}
				}
				onCompleted();
			}
			catch (Exception ex2)
			{
				if (onError == NopError)
				{
					UniTaskScheduler.PublishUnobservedTaskException(ex2);
					goto IL_0173;
				}
				if (ex2 is OperationCanceledException)
				{
					goto IL_0173;
				}
				onError(ex2);
			}
			goto end_IL_0038;
			IL_0173:
			num = 1;
			end_IL_0038:;
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
		if (num != 1)
		{
		}
	}

	public static async UniTaskVoid SubscribeAwaitCore<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask> onNext, Action<Exception> onError, Action onCompleted, CancellationToken cancellationToken)
	{
		IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		try
		{
			_ = 1;
			try
			{
				while (await e.MoveNextAsync())
				{
					try
					{
						await onNext(e.Current, cancellationToken);
					}
					catch (Exception ex)
					{
						UniTaskScheduler.PublishUnobservedTaskException(ex);
					}
				}
				onCompleted();
			}
			catch (Exception ex2)
			{
				if (onError == NopError)
				{
					UniTaskScheduler.PublishUnobservedTaskException(ex2);
					goto IL_017c;
				}
				if (ex2 is OperationCanceledException)
				{
					goto IL_017c;
				}
				onError(ex2);
			}
			goto end_IL_0038;
			IL_017c:
			num = 1;
			end_IL_0038:;
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
		if (num != 1)
		{
		}
	}
}
