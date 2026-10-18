using System;
using System.Threading;

namespace Cysharp.Threading.Tasks.Linq;

internal sealed class Do<TSource> : IUniTaskAsyncEnumerable<TSource>
{
	private sealed class _Do : MoveNextSource, IUniTaskAsyncEnumerator<TSource>, IUniTaskAsyncDisposable
	{
		private static readonly Action<object> MoveNextCoreDelegate = MoveNextCore;

		private readonly IUniTaskAsyncEnumerable<TSource> source;

		private readonly Action<TSource> onNext;

		private readonly Action<Exception> onError;

		private readonly Action onCompleted;

		private CancellationToken cancellationToken;

		private IUniTaskAsyncEnumerator<TSource> enumerator;

		private UniTask<bool>.Awaiter awaiter;

		public TSource Current { get; private set; }

		public _Do(IUniTaskAsyncEnumerable<TSource> source, Action<TSource> onNext, Action<Exception> onError, Action onCompleted, CancellationToken cancellationToken)
		{
			this.source = source;
			this.onNext = onNext;
			this.onError = onError;
			this.onCompleted = onCompleted;
			this.cancellationToken = cancellationToken;
		}

		public UniTask<bool> MoveNextAsync()
		{
			cancellationToken.ThrowIfCancellationRequested();
			completionSource.Reset();
			bool flag = false;
			try
			{
				if (enumerator == null)
				{
					enumerator = source.GetAsyncEnumerator(cancellationToken);
				}
				awaiter = enumerator.MoveNextAsync().GetAwaiter();
				flag = awaiter.IsCompleted;
			}
			catch (Exception ex)
			{
				CallTrySetExceptionAfterNotification(ex);
				return new UniTask<bool>(this, completionSource.Version);
			}
			if (flag)
			{
				MoveNextCore(this);
			}
			else
			{
				awaiter.SourceOnCompleted(MoveNextCoreDelegate, this);
			}
			return new UniTask<bool>(this, completionSource.Version);
		}

		private void CallTrySetExceptionAfterNotification(Exception ex)
		{
			if (onError != null)
			{
				try
				{
					onError(ex);
				}
				catch (Exception error)
				{
					completionSource.TrySetException(error);
					return;
				}
			}
			completionSource.TrySetException(ex);
		}

		private bool TryGetResultWithNotification<T>(UniTask<T>.Awaiter awaiter, out T result)
		{
			try
			{
				result = awaiter.GetResult();
				return true;
			}
			catch (Exception ex)
			{
				CallTrySetExceptionAfterNotification(ex);
				result = default(T);
				return false;
			}
		}

		private static void MoveNextCore(object state)
		{
			_Do @do = (_Do)state;
			if (!@do.TryGetResultWithNotification(@do.awaiter, out var result))
			{
				return;
			}
			if (result)
			{
				TSource current = @do.enumerator.Current;
				if (@do.onNext != null)
				{
					try
					{
						@do.onNext(current);
					}
					catch (Exception ex)
					{
						@do.CallTrySetExceptionAfterNotification(ex);
					}
				}
				@do.Current = current;
				@do.completionSource.TrySetResult(result: true);
				return;
			}
			if (@do.onCompleted != null)
			{
				try
				{
					@do.onCompleted();
				}
				catch (Exception ex2)
				{
					@do.CallTrySetExceptionAfterNotification(ex2);
					return;
				}
			}
			@do.completionSource.TrySetResult(result: false);
		}

		public UniTask DisposeAsync()
		{
			if (enumerator != null)
			{
				return enumerator.DisposeAsync();
			}
			return default(UniTask);
		}
	}

	private readonly IUniTaskAsyncEnumerable<TSource> source;

	private readonly Action<TSource> onNext;

	private readonly Action<Exception> onError;

	private readonly Action onCompleted;

	public Do(IUniTaskAsyncEnumerable<TSource> source, Action<TSource> onNext, Action<Exception> onError, Action onCompleted)
	{
		this.source = source;
		this.onNext = onNext;
		this.onError = onError;
		this.onCompleted = onCompleted;
	}

	public IUniTaskAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
	{
		return new _Do(source, onNext, onError, onCompleted, cancellationToken);
	}
}
