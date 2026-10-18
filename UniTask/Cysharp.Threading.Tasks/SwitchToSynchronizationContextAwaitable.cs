using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Cysharp.Threading.Tasks;

public struct SwitchToSynchronizationContextAwaitable
{
	public struct Awaiter : ICriticalNotifyCompletion, INotifyCompletion
	{
		private static readonly SendOrPostCallback switchToCallback = Callback;

		private readonly SynchronizationContext synchronizationContext;

		private readonly CancellationToken cancellationToken;

		public bool IsCompleted => false;

		public Awaiter(SynchronizationContext synchronizationContext, CancellationToken cancellationToken)
		{
			this.synchronizationContext = synchronizationContext;
			this.cancellationToken = cancellationToken;
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

	private readonly SynchronizationContext synchronizationContext;

	private readonly CancellationToken cancellationToken;

	public SwitchToSynchronizationContextAwaitable(SynchronizationContext synchronizationContext, CancellationToken cancellationToken)
	{
		this.synchronizationContext = synchronizationContext;
		this.cancellationToken = cancellationToken;
	}

	public Awaiter GetAwaiter()
	{
		return new Awaiter(synchronizationContext, cancellationToken);
	}
}
