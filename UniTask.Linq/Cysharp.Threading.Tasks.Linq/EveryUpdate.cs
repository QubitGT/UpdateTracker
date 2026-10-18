using System.Threading;

namespace Cysharp.Threading.Tasks.Linq;

internal class EveryUpdate : IUniTaskAsyncEnumerable<AsyncUnit>
{
	private class _EveryUpdate : MoveNextSource, IUniTaskAsyncEnumerator<AsyncUnit>, IUniTaskAsyncDisposable, IPlayerLoopItem
	{
		private readonly PlayerLoopTiming updateTiming;

		private CancellationToken cancellationToken;

		private bool disposed;

		public AsyncUnit Current => default(AsyncUnit);

		public _EveryUpdate(PlayerLoopTiming updateTiming, CancellationToken cancellationToken)
		{
			this.updateTiming = updateTiming;
			this.cancellationToken = cancellationToken;
			PlayerLoopHelper.AddAction(updateTiming, this);
		}

		public UniTask<bool> MoveNextAsync()
		{
			if (disposed || cancellationToken.IsCancellationRequested)
			{
				return CompletedTasks.False;
			}
			completionSource.Reset();
			return new UniTask<bool>(this, completionSource.Version);
		}

		public UniTask DisposeAsync()
		{
			if (!disposed)
			{
				disposed = true;
			}
			return default(UniTask);
		}

		public bool MoveNext()
		{
			if (disposed || cancellationToken.IsCancellationRequested)
			{
				completionSource.TrySetResult(result: false);
				return false;
			}
			completionSource.TrySetResult(result: true);
			return true;
		}
	}

	private readonly PlayerLoopTiming updateTiming;

	public EveryUpdate(PlayerLoopTiming updateTiming)
	{
		this.updateTiming = updateTiming;
	}

	public IUniTaskAsyncEnumerator<AsyncUnit> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
	{
		return new _EveryUpdate(updateTiming, cancellationToken);
	}
}
