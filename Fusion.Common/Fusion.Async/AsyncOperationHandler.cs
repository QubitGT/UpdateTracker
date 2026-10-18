using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fusion.Async;

internal class AsyncOperationHandler<T>
{
	private const float OperationTimeoutSec = 30f;

	private readonly TaskCompletionSource<T> _result;

	private readonly CancellationTokenSource _cancellation;

	private readonly string _customTimeoutMsg;

	public Task<T> Task => _result.Task;

	public AsyncOperationHandler(CancellationToken externalCancellationToken = default(CancellationToken), float operationTimeout = 30f, string customTimeoutMsg = null)
	{
		_result = new TaskCompletionSource<T>();
		_customTimeoutMsg = customTimeoutMsg;
		_cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(operationTimeout));
		_cancellation.Token.Register(Expire);
		if (externalCancellationToken != default(CancellationToken))
		{
			externalCancellationToken.Register(Cancel);
		}
	}

	public void SetResult(T result)
	{
		if (_result.TrySetResult(result))
		{
			if (!_cancellation.IsCancellationRequested)
			{
				_cancellation.Cancel();
			}
			_cancellation.Dispose();
		}
	}

	public void SetException(Exception e)
	{
		if (_result.TrySetException(e))
		{
			if (!_cancellation.IsCancellationRequested)
			{
				_cancellation.Cancel();
			}
			_cancellation.Dispose();
		}
	}

	private void Expire()
	{
		SetException(new TimeoutException("Operation timed out. " + _customTimeoutMsg));
	}

	private void Cancel()
	{
		SetException(new OperationCanceledException("Operation cancelled."));
	}
}
