using System;
using System.Threading;
using UnityEngine;

namespace Cysharp.Threading.Tasks.Linq;

internal class Timer : IUniTaskAsyncEnumerable<AsyncUnit>
{
	private class _Timer : MoveNextSource, IUniTaskAsyncEnumerator<AsyncUnit>, IUniTaskAsyncDisposable, IPlayerLoopItem
	{
		private readonly float dueTime;

		private readonly float? period;

		private readonly PlayerLoopTiming updateTiming;

		private readonly bool ignoreTimeScale;

		private CancellationToken cancellationToken;

		private int initialFrame;

		private float elapsed;

		private bool dueTimePhase;

		private bool completed;

		private bool disposed;

		public AsyncUnit Current => default(AsyncUnit);

		public _Timer(TimeSpan dueTime, TimeSpan? period, PlayerLoopTiming updateTiming, bool ignoreTimeScale, CancellationToken cancellationToken)
		{
			this.dueTime = (float)dueTime.TotalSeconds;
			this.period = ((!period.HasValue) ? null : new float?((float)period.Value.TotalSeconds));
			if (this.dueTime <= 0f)
			{
				this.dueTime = 0f;
			}
			if (this.period.HasValue && this.period <= 0f)
			{
				this.period = 1f;
			}
			initialFrame = (PlayerLoopHelper.IsMainThread ? Time.frameCount : (-1));
			dueTimePhase = true;
			this.updateTiming = updateTiming;
			this.ignoreTimeScale = ignoreTimeScale;
			this.cancellationToken = cancellationToken;
			PlayerLoopHelper.AddAction(updateTiming, this);
		}

		public UniTask<bool> MoveNextAsync()
		{
			if (disposed || cancellationToken.IsCancellationRequested || completed)
			{
				return CompletedTasks.False;
			}
			elapsed = 0f;
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
				if (elapsed == 0f && initialFrame == Time.frameCount)
				{
					return true;
				}
				elapsed += (ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);
				if (elapsed >= dueTime)
				{
					dueTimePhase = false;
					completionSource.TrySetResult(result: true);
				}
			}
			else
			{
				if (!period.HasValue)
				{
					completed = true;
					completionSource.TrySetResult(result: false);
					return false;
				}
				elapsed += (ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);
				if (elapsed >= period)
				{
					completionSource.TrySetResult(result: true);
				}
			}
			return true;
		}
	}

	private readonly PlayerLoopTiming updateTiming;

	private readonly TimeSpan dueTime;

	private readonly TimeSpan? period;

	private readonly bool ignoreTimeScale;

	public Timer(TimeSpan dueTime, TimeSpan? period, PlayerLoopTiming updateTiming, bool ignoreTimeScale)
	{
		this.updateTiming = updateTiming;
		this.dueTime = dueTime;
		this.period = period;
		this.ignoreTimeScale = ignoreTimeScale;
	}

	public IUniTaskAsyncEnumerator<AsyncUnit> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
	{
		return new _Timer(dueTime, period, updateTiming, ignoreTimeScale, cancellationToken);
	}
}
