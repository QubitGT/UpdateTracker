using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Cysharp.Threading.Tasks;

public struct ReturnToMainThread
{
	public readonly struct Awaiter : ICriticalNotifyCompletion, INotifyCompletion
	{
		private readonly PlayerLoopTiming timing;

		private readonly CancellationToken cancellationToken;

		public bool IsCompleted => PlayerLoopHelper.MainThreadId == Thread.CurrentThread.ManagedThreadId;

		public Awaiter(PlayerLoopTiming timing, CancellationToken cancellationToken)
		{
			this.timing = timing;
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
			PlayerLoopHelper.AddContinuation(timing, continuation);
		}

		public void UnsafeOnCompleted(Action continuation)
		{
			PlayerLoopHelper.AddContinuation(timing, continuation);
		}
	}

	private readonly PlayerLoopTiming playerLoopTiming;

	private readonly CancellationToken cancellationToken;

	public ReturnToMainThread(PlayerLoopTiming playerLoopTiming, CancellationToken cancellationToken)
	{
		this.playerLoopTiming = playerLoopTiming;
		this.cancellationToken = cancellationToken;
	}

	public Awaiter DisposeAsync()
	{
		return new Awaiter(playerLoopTiming, cancellationToken);
	}
}
