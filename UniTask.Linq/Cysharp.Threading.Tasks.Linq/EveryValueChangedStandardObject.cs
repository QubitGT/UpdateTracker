using System;
using System.Collections.Generic;
using System.Threading;

namespace Cysharp.Threading.Tasks.Linq;

internal sealed class EveryValueChangedStandardObject<TTarget, TProperty> : IUniTaskAsyncEnumerable<TProperty> where TTarget : class
{
	private sealed class _EveryValueChanged : MoveNextSource, IUniTaskAsyncEnumerator<TProperty>, IUniTaskAsyncDisposable, IPlayerLoopItem
	{
		private readonly WeakReference<TTarget> target;

		private readonly IEqualityComparer<TProperty> equalityComparer;

		private readonly Func<TTarget, TProperty> propertySelector;

		private CancellationToken cancellationToken;

		private bool first;

		private TProperty currentValue;

		private bool disposed;

		public TProperty Current => currentValue;

		public _EveryValueChanged(WeakReference<TTarget> target, Func<TTarget, TProperty> propertySelector, IEqualityComparer<TProperty> equalityComparer, PlayerLoopTiming monitorTiming, CancellationToken cancellationToken)
		{
			this.target = target;
			this.propertySelector = propertySelector;
			this.equalityComparer = equalityComparer;
			this.cancellationToken = cancellationToken;
			first = true;
			PlayerLoopHelper.AddAction(monitorTiming, this);
		}

		public UniTask<bool> MoveNextAsync()
		{
			if (disposed || cancellationToken.IsCancellationRequested)
			{
				return CompletedTasks.False;
			}
			if (first)
			{
				first = false;
				if (!target.TryGetTarget(out var arg))
				{
					return CompletedTasks.False;
				}
				currentValue = propertySelector(arg);
				return CompletedTasks.True;
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
			if (disposed || cancellationToken.IsCancellationRequested || !target.TryGetTarget(out var arg))
			{
				completionSource.TrySetResult(result: false);
				DisposeAsync().Forget();
				return false;
			}
			TProperty val = default(TProperty);
			try
			{
				val = propertySelector(arg);
				if (equalityComparer.Equals(currentValue, val))
				{
					return true;
				}
			}
			catch (Exception error)
			{
				completionSource.TrySetException(error);
				DisposeAsync().Forget();
				return false;
			}
			currentValue = val;
			completionSource.TrySetResult(result: true);
			return true;
		}
	}

	private readonly WeakReference<TTarget> target;

	private readonly Func<TTarget, TProperty> propertySelector;

	private readonly IEqualityComparer<TProperty> equalityComparer;

	private readonly PlayerLoopTiming monitorTiming;

	public EveryValueChangedStandardObject(TTarget target, Func<TTarget, TProperty> propertySelector, IEqualityComparer<TProperty> equalityComparer, PlayerLoopTiming monitorTiming)
	{
		this.target = new WeakReference<TTarget>(target, trackResurrection: false);
		this.propertySelector = propertySelector;
		this.equalityComparer = equalityComparer;
		this.monitorTiming = monitorTiming;
	}

	public IUniTaskAsyncEnumerator<TProperty> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
	{
		return new _EveryValueChanged(target, propertySelector, equalityComparer, monitorTiming, cancellationToken);
	}
}
