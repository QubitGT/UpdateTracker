using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Cysharp.Threading.Tasks;

public readonly struct YieldAwaitable
{
	public readonly struct Awaiter : ICriticalNotifyCompletion, INotifyCompletion
	{
		private readonly PlayerLoopTiming timing;

		public bool IsCompleted => false;

		public Awaiter(PlayerLoopTiming timing)
		{
			this.timing = timing;
		}

		public void GetResult()
		{
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

	private readonly PlayerLoopTiming timing;

	public YieldAwaitable(PlayerLoopTiming timing)
	{
		this.timing = timing;
	}

	public Awaiter GetAwaiter()
	{
		return new Awaiter(timing);
	}

	public UniTask ToUniTask()
	{
		return UniTask.Yield(timing, CancellationToken.None);
	}
}
