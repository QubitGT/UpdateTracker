#define TRACE
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Fusion;

public readonly struct NetworkSceneAsyncOp : IEnumerator
{
	public struct Awaiter : INotifyCompletion
	{
		private NetworkSceneAsyncOp _op;

		public bool IsCompleted => _op.IsDone;

		public Awaiter(in NetworkSceneAsyncOp op)
		{
			_op = op;
		}

		public void GetResult()
		{
			if (!IsCompleted)
			{
				SpinWait spinWait = default(SpinWait);
				while (!IsCompleted)
				{
					spinWait.SpinOnce();
				}
			}
			_op.ThrowIfError();
		}

		public void OnCompleted(Action continuation)
		{
			if (IsCompleted)
			{
				continuation();
				return;
			}
			SynchronizationContext capturedContext = SynchronizationContext.Current;
			_op.AddOnCompleted(delegate
			{
				if (capturedContext != null)
				{
					capturedContext.Post(delegate
					{
						continuation();
					}, null);
				}
				else
				{
					continuation();
				}
			});
		}
	}

	public readonly SceneRef SceneRef;

	private readonly object _data;

	public bool IsValid => SceneRef != default(SceneRef);

	public bool IsDone
	{
		get
		{
			object data = _data;
			object obj = data;
			if (!(obj is AsyncOperation { isDone: var isDone }))
			{
				if (!(obj is ICoroutine { IsDone: var isDone2 }))
				{
					if (!(obj is ExceptionDispatchInfo))
					{
						if (!(obj is Task { IsCompleted: var isCompleted }))
						{
							return true;
						}
						return isCompleted;
					}
					return true;
				}
				return isDone2;
			}
			return isDone;
		}
	}

	public Exception Error
	{
		get
		{
			object data = _data;
			object obj = data;
			if (!(obj is ICoroutine coroutine))
			{
				if (!(obj is Task task))
				{
					if (!(obj is ExceptionDispatchInfo { SourceException: var sourceException }))
					{
						return null;
					}
					return sourceException;
				}
				return task.Exception;
			}
			return coroutine.Error?.SourceException;
		}
	}

	object IEnumerator.Current => null;

	private NetworkSceneAsyncOp(SceneRef sceneRef, object data)
	{
		SceneRef = sceneRef;
		_data = data ?? throw new ArgumentNullException("data");
	}

	private NetworkSceneAsyncOp(SceneRef sceneRef)
	{
		SceneRef = sceneRef;
		_data = null;
	}

	internal void ThrowIfError()
	{
		object data = _data;
		object obj = data;
		if (!(obj is ICoroutine coroutine))
		{
			if (!(obj is ExceptionDispatchInfo exceptionDispatchInfo))
			{
				if (obj is Task { IsFaulted: not false } task)
				{
					task.GetAwaiter().GetResult();
					Assert.AlwaysFail("Expected to have thrown");
				}
			}
			else
			{
				exceptionDispatchInfo.Throw();
				Assert.AlwaysFail("Expected to have thrown");
			}
		}
		else if (coroutine.Error != null)
		{
			coroutine.Error.Throw();
			Assert.AlwaysFail("Expected to have thrown");
		}
	}

	public static NetworkSceneAsyncOp FromAsyncOperation(SceneRef sceneRef, AsyncOperation asyncOp)
	{
		if (asyncOp == null)
		{
			throw new ArgumentNullException("asyncOp");
		}
		return new NetworkSceneAsyncOp(sceneRef, asyncOp);
	}

	public static NetworkSceneAsyncOp FromCoroutine(SceneRef sceneRef, ICoroutine coroutine)
	{
		if (coroutine == null)
		{
			throw new ArgumentNullException("coroutine");
		}
		return new NetworkSceneAsyncOp(sceneRef, coroutine);
	}

	public static NetworkSceneAsyncOp FromTask(SceneRef sceneRef, Task task)
	{
		if (task == null)
		{
			throw new ArgumentNullException("task");
		}
		return new NetworkSceneAsyncOp(sceneRef, task);
	}

	public static NetworkSceneAsyncOp FromError(SceneRef sceneRef, Exception error)
	{
		if (error == null)
		{
			throw new ArgumentNullException("error");
		}
		return new NetworkSceneAsyncOp(sceneRef, ExceptionDispatchInfo.Capture(error));
	}

	public static NetworkSceneAsyncOp FromCompleted(SceneRef sceneRef)
	{
		return new NetworkSceneAsyncOp(sceneRef);
	}

	internal static NetworkSceneAsyncOp FromDeferred(SceneRef sceneRef, Task blockingTask, Func<SceneRef, NetworkSceneAsyncOp> op)
	{
		return FromTask(sceneRef, CreateDeferredOpTask(sceneRef, blockingTask, op));
	}

	private static async Task CreateDeferredOpTask(SceneRef sceneRef, Task blockingTask, Func<SceneRef, NetworkSceneAsyncOp> op)
	{
		InternalLogStreams.LogTraceSceneManager?.Log($"Awaiting blocking task for {sceneRef}");
		await blockingTask;
		InternalLogStreams.LogTraceSceneManager?.Log($"Awaited blocking task for {sceneRef}, loading");
		await op(sceneRef);
		InternalLogStreams.LogTraceSceneManager?.Log("Awaited loading");
	}

	public void AddOnCompleted(Action<NetworkSceneAsyncOp> action)
	{
		NetworkSceneAsyncOp captured = this;
		object data = _data;
		object obj = data;
		if (!(obj is AsyncOperation asyncOperation))
		{
			if (!(obj is ICoroutine coroutine))
			{
				if (!(obj is Task task))
				{
					if (obj is ExceptionDispatchInfo)
					{
						action(captured);
					}
					else
					{
						action(captured);
					}
				}
				else
				{
					task.ContinueWith(delegate
					{
						action(captured);
					});
				}
			}
			else
			{
				coroutine.Completed += delegate
				{
					action(captured);
				};
			}
		}
		else
		{
			asyncOperation.completed += delegate
			{
				action(captured);
			};
		}
	}

	public Awaiter GetAwaiter()
	{
		return new Awaiter(in this);
	}

	bool IEnumerator.MoveNext()
	{
		return !IsDone;
	}

	void IEnumerator.Reset()
	{
	}
}
