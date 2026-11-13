using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Cysharp.Threading.Tasks.Linq;

internal sealed class Publish<TSource> : IConnectableUniTaskAsyncEnumerable<TSource>, IUniTaskAsyncEnumerable<TSource>
{
	private sealed class ConnectDisposable : IDisposable
	{
		private readonly CancellationTokenSource cancellationTokenSource;

		public ConnectDisposable(CancellationTokenSource cancellationTokenSource)
		{
			this.cancellationTokenSource = cancellationTokenSource;
		}

		public void Dispose()
		{
			cancellationTokenSource.Cancel();
		}
	}

	private sealed class _Publish : MoveNextSource, IUniTaskAsyncEnumerator<TSource>, IUniTaskAsyncDisposable, ITriggerHandler<TSource>
	{
		private static readonly Action<object> CancelDelegate = OnCanceled;

		private readonly Publish<TSource> parent;

		private CancellationToken cancellationToken;

		private CancellationTokenRegistration cancellationTokenRegistration;

		private bool isDisposed;

		public TSource Current { get; private set; }

		ITriggerHandler<TSource> ITriggerHandler<TSource>.Prev { get; set; }

		ITriggerHandler<TSource> ITriggerHandler<TSource>.Next { get; set; }

		public _Publish(Publish<TSource> parent, CancellationToken cancellationToken)
		{
			if (!cancellationToken.IsCancellationRequested)
			{
				this.parent = parent;
				this.cancellationToken = cancellationToken;
				if (cancellationToken.CanBeCanceled)
				{
					cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(CancelDelegate, this);
				}
				parent.trigger.Add(this);
			}
		}

		public UniTask<bool> MoveNextAsync()
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (parent.isCompleted)
			{
				return CompletedTasks.False;
			}
			completionSource.Reset();
			return new UniTask<bool>(this, completionSource.Version);
		}

		private static void OnCanceled(object state)
		{
			_Publish publish = (_Publish)state;
			publish.completionSource.TrySetCanceled(publish.cancellationToken);
			publish.DisposeAsync().Forget();
		}

		public UniTask DisposeAsync()
		{
			if (!isDisposed)
			{
				isDisposed = true;
				cancellationTokenRegistration.Dispose();
				parent.trigger.Remove(this);
			}
			return default(UniTask);
		}

		public void OnNext(TSource value)
		{
			Current = value;
			completionSource.TrySetResult(result: true);
		}

		public void OnCanceled(CancellationToken cancellationToken)
		{
			completionSource.TrySetCanceled(cancellationToken);
		}

		public void OnCompleted()
		{
			completionSource.TrySetResult(result: false);
		}

		public void OnError(Exception ex)
		{
			completionSource.TrySetException(ex);
		}
	}

	private readonly IUniTaskAsyncEnumerable<TSource> source;

	private readonly CancellationTokenSource cancellationTokenSource;

	private TriggerEvent<TSource> trigger;

	private IUniTaskAsyncEnumerator<TSource> enumerator;

	private IDisposable connectedDisposable;

	private bool isCompleted;

	public Publish(IUniTaskAsyncEnumerable<TSource> source)
	{
		this.source = source;
		cancellationTokenSource = new CancellationTokenSource();
	}

	public IDisposable Connect()
	{
		if (connectedDisposable != null)
		{
			return connectedDisposable;
		}
		if (enumerator == null)
		{
			enumerator = source.GetAsyncEnumerator(cancellationTokenSource.Token);
		}
		ConsumeEnumerator().Forget();
		connectedDisposable = new ConnectDisposable(cancellationTokenSource);
		return connectedDisposable;
	}

	private async UniTaskVoid ConsumeEnumerator()
	{
		object obj = null;
		try
		{
			try
			{
				while (await enumerator.MoveNextAsync())
				{
					trigger.SetResult(enumerator.Current);
				}
				trigger.SetCompleted();
			}
			catch (Exception error)
			{
				trigger.SetError(error);
			}
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		isCompleted = true;
		await enumerator.DisposeAsync();
		object obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
	}

	public IUniTaskAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
	{
		return new _Publish(this, cancellationToken);
	}
}
