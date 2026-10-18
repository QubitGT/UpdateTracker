using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Cysharp.Threading.Tasks;

public struct CancellationTokenAwaitable
{
	public struct Awaiter : ICriticalNotifyCompletion, INotifyCompletion
	{
		private CancellationToken cancellationToken;

		public bool IsCompleted
		{
			get
			{
				if (cancellationToken.CanBeCanceled)
				{
					return cancellationToken.IsCancellationRequested;
				}
				return true;
			}
		}

		public Awaiter(CancellationToken cancellationToken)
		{
			this.cancellationToken = cancellationToken;
		}

		public void GetResult()
		{
		}

		public void OnCompleted(Action continuation)
		{
			UnsafeOnCompleted(continuation);
		}

		public void UnsafeOnCompleted(Action continuation)
		{
			cancellationToken.RegisterWithoutCaptureExecutionContext(continuation);
		}
	}

	private CancellationToken cancellationToken;

	public CancellationTokenAwaitable(CancellationToken cancellationToken)
	{
		this.cancellationToken = cancellationToken;
	}

	public Awaiter GetAwaiter()
	{
		return new Awaiter(cancellationToken);
	}
}
