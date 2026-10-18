using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Cysharp.Threading.Tasks.Linq;

internal sealed class EveryValueChangedUnityObject<TTarget, TProperty> : IUniTaskAsyncEnumerable<TProperty>
{
	private sealed class _EveryValueChanged : MoveNextSource, IUniTaskAsyncEnumerator<TProperty>, IUniTaskAsyncDisposable, IPlayerLoopItem
	{
		private readonly TTarget target;

		private readonly UnityEngine.Object targetAsUnityObject;

		private readonly IEqualityComparer<TProperty> equalityComparer;

		private readonly Func<TTarget, TProperty> propertySelector;

		private CancellationToken cancellationToken;

		private bool first;

		private TProperty currentValue;

		private bool disposed;

		public TProperty Current => currentValue;

		public _EveryValueChanged(TTarget target, Func<TTarget, TProperty> propertySelector, IEqualityComparer<TProperty> equalityComparer, PlayerLoopTiming monitorTiming, CancellationToken cancellationToken)
		{
			this.target = target;
			targetAsUnityObject = target as UnityEngine.Object;
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
				if (targetAsUnityObject == null)
				{
					return CompletedTasks.False;
				}
				currentValue = propertySelector(target);
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
			if (disposed || cancellationToken.IsCancellationRequested || targetAsUnityObject == null)
			{
				completionSource.TrySetResult(result: false);
				DisposeAsync().Forget();
				return false;
			}
			TProperty val = default(TProperty);
			try
			{
				val = propertySelector(target);
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

	private readonly TTarget target;

	private readonly Func<TTarget, TProperty> propertySelector;

	private readonly IEqualityComparer<TProperty> equalityComparer;

	private readonly PlayerLoopTiming monitorTiming;

	public EveryValueChangedUnityObject(TTarget target, Func<TTarget, TProperty> propertySelector, IEqualityComparer<TProperty> equalityComparer, PlayerLoopTiming monitorTiming)
	{
		this.target = target;
		this.propertySelector = propertySelector;
		this.equalityComparer = equalityComparer;
		this.monitorTiming = monitorTiming;
	}

	public IUniTaskAsyncEnumerator<TProperty> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
	{
		return new _EveryValueChanged(target, propertySelector, equalityComparer, monitorTiming, cancellationToken);
	}
}
