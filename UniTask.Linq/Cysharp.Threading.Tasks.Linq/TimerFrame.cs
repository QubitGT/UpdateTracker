using System.Threading;
using UnityEngine;

namespace Cysharp.Threading.Tasks.Linq;

internal class TimerFrame : IUniTaskAsyncEnumerable<AsyncUnit>
{
	private class _TimerFrame : MoveNextSource, IUniTaskAsyncEnumerator<AsyncUnit>, IUniTaskAsyncDisposable, IPlayerLoopItem
	{
		private readonly int dueTimeFrameCount;

		private readonly int? periodFrameCount;

		private CancellationToken cancellationToken;

		private int initialFrame;

		private int currentFrame;

		private bool dueTimePhase;

		private bool completed;

		private bool disposed;

		public AsyncUnit Current => default(AsyncUnit);

		public _TimerFrame(int dueTimeFrameCount, int? periodFrameCount, PlayerLoopTiming updateTiming, CancellationToken cancellationToken)
		{
			if (dueTimeFrameCount <= 0)
			{
				dueTimeFrameCount = 0;
			}
			if (periodFrameCount.HasValue && periodFrameCount <= 0)
			{
				periodFrameCount = 1;
			}
			initialFrame = (PlayerLoopHelper.IsMainThread ? Time.frameCount : (-1));
			dueTimePhase = true;
			this.dueTimeFrameCount = dueTimeFrameCount;
			this.periodFrameCount = periodFrameCount;
			this.cancellationToken = cancellationToken;
			PlayerLoopHelper.AddAction(updateTiming, this);
		}

		public UniTask<bool> MoveNextAsync()
		{
			if (disposed || cancellationToken.IsCancellationRequested || completed)
			{
				return CompletedTasks.False;
			}
			currentFrame = 0;
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
			if (dueTimePhase)
			{
				if (currentFrame == 0)
				{
					if (dueTimeFrameCount == 0)
					{
						dueTimePhase = false;
						completionSource.TrySetResult(result: true);
						return true;
					}
					if (initialFrame == Time.frameCount)
					{
						return true;
					}
				}
				if (++currentFrame >= dueTimeFrameCount)
				{
					dueTimePhase = false;
					completionSource.TrySetResult(result: true);
				}
			}
			else
			{
				if (!periodFrameCount.HasValue)
				{
					completed = true;
					completionSource.TrySetResult(result: false);
					return false;
				}
				if (++currentFrame >= periodFrameCount)
				{
					completionSource.TrySetResult(result: true);
				}
			}
			return true;
		}
	}

	private readonly PlayerLoopTiming updateTiming;

	private readonly int dueTimeFrameCount;

	private readonly int? periodFrameCount;

	public TimerFrame(int dueTimeFrameCount, int? periodFrameCount, PlayerLoopTiming updateTiming)
	{
		this.updateTiming = updateTiming;
		this.dueTimeFrameCount = dueTimeFrameCount;
		this.periodFrameCount = periodFrameCount;
	}

	public IUniTaskAsyncEnumerator<AsyncUnit> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
	{
		return new _TimerFrame(dueTimeFrameCount, periodFrameCount, updateTiming, cancellationToken);
	}
}
