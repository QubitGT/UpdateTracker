using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Cysharp.Threading.Tasks;

public struct SwitchToMainThreadAwaitable
{
	public struct Awaiter : ICriticalNotifyCompletion, INotifyCompletion
	{
		private readonly PlayerLoopTiming playerLoopTiming;

		private readonly CancellationToken cancellationToken;

		public bool IsCompleted
		{
			get
			{
				int managedThreadId = Thread.CurrentThread.ManagedThreadId;
				if (PlayerLoopHelper.MainThreadId == managedThreadId)
				{
					return true;
				}
				return false;
			}
		}

		public Awaiter(PlayerLoopTiming playerLoopTiming, CancellationToken cancellationToken)
		{
			this.playerLoopTiming = playerLoopTiming;
			this.cancellationToken = cancellationToken;
		}

		public void GetResult()
		{
			cancellationToken.ThrowIfCancellationRequested();
		}

		public void OnCompleted(Action continuation)
		{
			PlayerLoopHelper.AddContinuation(playerLoopTiming, continuation);
		}

		public void UnsafeOnCompleted(Action continuation)
		{
			PlayerLoopHelper.AddContinuation(playerLoopTiming, continuation);
		}
	}

	private readonly PlayerLoopTiming playerLoopTiming;

	private readonly CancellationToken cancellationToken;

	public SwitchToMainThreadAwaitable(PlayerLoopTiming playerLoopTiming, CancellationToken cancellationToken)
	{
		this.playerLoopTiming = playerLoopTiming;
		this.cancellationToken = cancellationToken;
	}

	public Awaiter GetAwaiter()
	{
		return new Awaiter(playerLoopTiming, cancellationToken);
	}
}
