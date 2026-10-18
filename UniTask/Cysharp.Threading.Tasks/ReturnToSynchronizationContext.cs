using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Cysharp.Threading.Tasks;

public struct ReturnToSynchronizationContext
{
	public struct Awaiter : ICriticalNotifyCompletion, INotifyCompletion
	{
		private static readonly SendOrPostCallback switchToCallback = Callback;

		private readonly SynchronizationContext synchronizationContext;

		private readonly bool dontPostWhenSameContext;

		private readonly CancellationToken cancellationToken;

		public bool IsCompleted
		{
			get
			{
				if (!dontPostWhenSameContext)
				{
					return false;
				}
				if (SynchronizationContext.Current == synchronizationContext)
				{
					return true;
				}
				return false;
			}
		}

		public Awaiter(SynchronizationContext synchronizationContext, bool dontPostWhenSameContext, CancellationToken cancellationToken)
		{
			this.synchronizationContext = synchronizationContext;
			this.dontPostWhenSameContext = dontPostWhenSameContext;
			this.cancellationToken = cancellationToken;
		}

		public Awaiter GetAwaiter()
		{
			return this;
		}

		public void GetResult()
		{
			cancellationToken.ThrowIfCancellationRequested();
		}

		public void OnCompleted(Action continuation)
		{
			synchronizationContext.Post(switchToCallback, continuation);
		}

		public void UnsafeOnCompleted(Action continuation)
		{
			synchronizationContext.Post(switchToCallback, continuation);
		}

		private static void Callback(object state)
		{
			((Action)state)();
		}
	}

	private readonly SynchronizationContext syncContext;

	private readonly bool dontPostWhenSameContext;

	private readonly CancellationToken cancellationToken;

	public ReturnToSynchronizationContext(SynchronizationContext syncContext, bool dontPostWhenSameContext, CancellationToken cancellationToken)
	{
		this.syncContext = syncContext;
		this.dontPostWhenSameContext = dontPostWhenSameContext;
		this.cancellationToken = cancellationToken;
	}

	public Awaiter DisposeAsync()
	{
		return new Awaiter(syncContext, dontPostWhenSameContext, cancellationToken);
	}
}
