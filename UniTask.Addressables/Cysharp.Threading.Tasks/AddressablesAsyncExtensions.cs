using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using Cysharp.Threading.Tasks.Internal;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Cysharp.Threading.Tasks;

public static class AddressablesAsyncExtensions
{
	public struct AsyncOperationHandleAwaiter : ICriticalNotifyCompletion, INotifyCompletion
	{
		private AsyncOperationHandle handle;

		private Action<AsyncOperationHandle> continuationAction;

		public bool IsCompleted => handle.IsDone;

		public AsyncOperationHandleAwaiter(AsyncOperationHandle handle)
		{
			this.handle = handle;
			continuationAction = null;
		}

		public void GetResult()
		{
			if (continuationAction != null)
			{
				handle.Completed -= continuationAction;
				continuationAction = null;
			}
			if (handle.Status == AsyncOperationStatus.Failed)
			{
				Exception operationException = handle.OperationException;
				handle = default(AsyncOperationHandle);
				ExceptionDispatchInfo.Capture(operationException).Throw();
			}
			_ = handle.Result;
			handle = default(AsyncOperationHandle);
		}

		public void OnCompleted(Action continuation)
		{
			UnsafeOnCompleted(continuation);
		}

		public void UnsafeOnCompleted(Action continuation)
		{
			Error.ThrowWhenContinuationIsAlreadyRegistered(continuationAction);
			continuationAction = PooledDelegate<AsyncOperationHandle>.Create(continuation);
			handle.Completed += continuationAction;
		}
	}

	private sealed class AsyncOperationHandleConfiguredSource : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<AsyncOperationHandleConfiguredSource>
	{
		private static TaskPool<AsyncOperationHandleConfiguredSource> pool;

		private AsyncOperationHandleConfiguredSource nextNode;

		private readonly Action<AsyncOperationHandle> continuationAction;

		private AsyncOperationHandle handle;

		private CancellationToken cancellationToken;

		private IProgress<float> progress;

		private bool completed;

		private UniTaskCompletionSourceCore<AsyncUnit> core;

		public ref AsyncOperationHandleConfiguredSource NextNode => ref nextNode;

		static AsyncOperationHandleConfiguredSource()
		{
			TaskPool.RegisterSizeGetter(typeof(AsyncOperationHandleConfiguredSource), () => pool.Size);
		}

		private AsyncOperationHandleConfiguredSource()
		{
			continuationAction = Continuation;
		}

		public static IUniTaskSource Create(AsyncOperationHandle handle, PlayerLoopTiming timing, IProgress<float> progress, CancellationToken cancellationToken, out short token)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
			}
			if (!pool.TryPop(out var result))
			{
				result = new AsyncOperationHandleConfiguredSource();
			}
			result.handle = handle;
			result.progress = progress;
			result.cancellationToken = cancellationToken;
			result.completed = false;
			PlayerLoopHelper.AddAction(timing, result);
			handle.Completed += result.continuationAction;
			token = result.core.Version;
			return result;
		}

		private void Continuation(AsyncOperationHandle _)
		{
			handle.Completed -= continuationAction;
			if (completed)
			{
				TryReturn();
				return;
			}
			completed = true;
			if (cancellationToken.IsCancellationRequested)
			{
				core.TrySetCanceled(cancellationToken);
			}
			else if (handle.Status == AsyncOperationStatus.Failed)
			{
				core.TrySetException(handle.OperationException);
			}
			else
			{
				core.TrySetResult(AsyncUnit.Default);
			}
		}

		public void GetResult(short token)
		{
			core.GetResult(token);
		}

		public UniTaskStatus GetStatus(short token)
		{
			return core.GetStatus(token);
		}

		public UniTaskStatus UnsafeGetStatus()
		{
			return core.UnsafeGetStatus();
		}

		public void OnCompleted(Action<object> continuation, object state, short token)
		{
			core.OnCompleted(continuation, state, token);
		}

		public bool MoveNext()
		{
			if (completed)
			{
				TryReturn();
				return false;
			}
			if (cancellationToken.IsCancellationRequested)
			{
				completed = true;
				core.TrySetCanceled(cancellationToken);
				return false;
			}
			if (progress != null && handle.IsValid())
			{
				progress.Report(handle.PercentComplete);
			}
			return true;
		}

		private bool TryReturn()
		{
			core.Reset();
			handle = default(AsyncOperationHandle);
			progress = null;
			cancellationToken = default(CancellationToken);
			return pool.TryPush(this);
		}
	}

	private sealed class AsyncOperationHandleConfiguredSource<T> : IUniTaskSource<T>, IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<AsyncOperationHandleConfiguredSource<T>>
	{
		private static TaskPool<AsyncOperationHandleConfiguredSource<T>> pool;

		private AsyncOperationHandleConfiguredSource<T> nextNode;

		private readonly Action<AsyncOperationHandle<T>> continuationAction;

		private AsyncOperationHandle<T> handle;

		private CancellationToken cancellationToken;

		private IProgress<float> progress;

		private bool completed;

		private UniTaskCompletionSourceCore<T> core;

		public ref AsyncOperationHandleConfiguredSource<T> NextNode => ref nextNode;

		static AsyncOperationHandleConfiguredSource()
		{
			TaskPool.RegisterSizeGetter(typeof(AsyncOperationHandleConfiguredSource<T>), () => pool.Size);
		}

		private AsyncOperationHandleConfiguredSource()
		{
			continuationAction = Continuation;
		}

		public static IUniTaskSource<T> Create(AsyncOperationHandle<T> handle, PlayerLoopTiming timing, IProgress<float> progress, CancellationToken cancellationToken, out short token)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return AutoResetUniTaskCompletionSource<T>.CreateFromCanceled(cancellationToken, out token);
			}
			if (!pool.TryPop(out var result))
			{
				result = new AsyncOperationHandleConfiguredSource<T>();
			}
			result.handle = handle;
			result.cancellationToken = cancellationToken;
			result.completed = false;
			result.progress = progress;
			PlayerLoopHelper.AddAction(timing, result);
			handle.Completed += result.continuationAction;
			token = result.core.Version;
			return result;
		}

		private void Continuation(AsyncOperationHandle<T> argHandle)
		{
			handle.Completed -= continuationAction;
			if (completed)
			{
				TryReturn();
				return;
			}
			completed = true;
			if (cancellationToken.IsCancellationRequested)
			{
				core.TrySetCanceled(cancellationToken);
			}
			else if (argHandle.Status == AsyncOperationStatus.Failed)
			{
				core.TrySetException(argHandle.OperationException);
			}
			else
			{
				core.TrySetResult(argHandle.Result);
			}
		}

		public T GetResult(short token)
		{
			return core.GetResult(token);
		}

		void IUniTaskSource.GetResult(short token)
		{
			GetResult(token);
		}

		public UniTaskStatus GetStatus(short token)
		{
			return core.GetStatus(token);
		}

		public UniTaskStatus UnsafeGetStatus()
		{
			return core.UnsafeGetStatus();
		}

		public void OnCompleted(Action<object> continuation, object state, short token)
		{
			core.OnCompleted(continuation, state, token);
		}

		public bool MoveNext()
		{
			if (completed)
			{
				TryReturn();
				return false;
			}
			if (cancellationToken.IsCancellationRequested)
			{
				completed = true;
				core.TrySetCanceled(cancellationToken);
				return false;
			}
			if (progress != null && handle.IsValid())
			{
				progress.Report(handle.PercentComplete);
			}
			return true;
		}

		private bool TryReturn()
		{
			core.Reset();
			handle = default(AsyncOperationHandle<T>);
			progress = null;
			cancellationToken = default(CancellationToken);
			return pool.TryPush(this);
		}
	}

	public static UniTask.Awaiter GetAwaiter(this AsyncOperationHandle handle)
	{
		return handle.ToUniTask().GetAwaiter();
	}

	public static UniTask WithCancellation(this AsyncOperationHandle handle, CancellationToken cancellationToken)
	{
		return handle.ToUniTask(null, PlayerLoopTiming.Update, cancellationToken);
	}

	public static UniTask ToUniTask(this AsyncOperationHandle handle, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return UniTask.FromCanceled(cancellationToken);
		}
		if (!handle.IsValid())
		{
			return UniTask.CompletedTask;
		}
		if (handle.IsDone)
		{
			if (handle.Status == AsyncOperationStatus.Failed)
			{
				return UniTask.FromException(handle.OperationException);
			}
			return UniTask.CompletedTask;
		}
		short token;
		return new UniTask(AsyncOperationHandleConfiguredSource.Create(handle, timing, progress, cancellationToken, out token), token);
	}

	public static UniTask<T>.Awaiter GetAwaiter<T>(this AsyncOperationHandle<T> handle)
	{
		return handle.ToUniTask().GetAwaiter();
	}

	public static UniTask<T> WithCancellation<T>(this AsyncOperationHandle<T> handle, CancellationToken cancellationToken)
	{
		return handle.ToUniTask(null, PlayerLoopTiming.Update, cancellationToken);
	}

	public static UniTask<T> ToUniTask<T>(this AsyncOperationHandle<T> handle, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return UniTask.FromCanceled<T>(cancellationToken);
		}
		if (!handle.IsValid())
		{
			throw new Exception("Attempting to use an invalid operation handle");
		}
		if (handle.IsDone)
		{
			if (handle.Status == AsyncOperationStatus.Failed)
			{
				return UniTask.FromException<T>(handle.OperationException);
			}
			return UniTask.FromResult(handle.Result);
		}
		short token;
		return new UniTask<T>(AsyncOperationHandleConfiguredSource<T>.Create(handle, timing, progress, cancellationToken, out token), token);
	}
}
