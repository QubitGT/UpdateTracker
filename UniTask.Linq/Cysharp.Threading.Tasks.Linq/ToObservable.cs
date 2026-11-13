using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Cysharp.Threading.Tasks.Linq;

internal sealed class ToObservable<T> : IObservable<T>
{
	internal sealed class CancellationTokenDisposable : IDisposable
	{
		private readonly CancellationTokenSource cts = new CancellationTokenSource();

		public CancellationToken Token => cts.Token;

		public void Dispose()
		{
			if (!cts.IsCancellationRequested)
			{
				cts.Cancel();
			}
		}
	}

	private readonly IUniTaskAsyncEnumerable<T> source;

	public ToObservable(IUniTaskAsyncEnumerable<T> source)
	{
		this.source = source;
	}

	public IDisposable Subscribe(IObserver<T> observer)
	{
		CancellationTokenDisposable cancellationTokenDisposable = new CancellationTokenDisposable();
		RunAsync(source, observer, cancellationTokenDisposable.Token).Forget();
		return cancellationTokenDisposable;
	}

	private static async UniTaskVoid RunAsync(IUniTaskAsyncEnumerable<T> src, IObserver<T> observer, CancellationToken cancellationToken)
	{
		IUniTaskAsyncEnumerator<T> e = src.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		try
		{
			while (true)
			{
				bool flag;
				try
				{
					flag = await e.MoveNextAsync();
				}
				catch (Exception error)
				{
					if (!cancellationToken.IsCancellationRequested)
					{
						observer.OnError(error);
					}
					goto IL_00fd;
				}
				if (flag)
				{
					observer.OnNext(e.Current);
					if (cancellationToken.IsCancellationRequested)
					{
						break;
					}
					continue;
				}
				observer.OnCompleted();
				goto IL_00fd;
				IL_00fd:
				num = 1;
				break;
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
		if (num != 1)
		{
		}
	}
}
