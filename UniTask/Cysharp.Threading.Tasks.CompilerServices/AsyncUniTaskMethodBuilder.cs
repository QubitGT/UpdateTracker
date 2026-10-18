using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace Cysharp.Threading.Tasks.CompilerServices;

[StructLayout(LayoutKind.Auto)]
public struct AsyncUniTaskMethodBuilder
{
	private IStateMachineRunnerPromise runnerPromise;

	private Exception ex;

	public UniTask Task
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[DebuggerHidden]
		get
		{
			if (runnerPromise != null)
			{
				return runnerPromise.Task;
			}
			if (ex != null)
			{
				return UniTask.FromException(ex);
			}
			return UniTask.CompletedTask;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[DebuggerHidden]
	public static AsyncUniTaskMethodBuilder Create()
	{
		return default(AsyncUniTaskMethodBuilder);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[DebuggerHidden]
	public void SetException(Exception exception)
	{
		if (runnerPromise == null)
		{
			ex = exception;
		}
		else
		{
			runnerPromise.SetException(exception);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[DebuggerHidden]
	public void SetResult()
	{
		if (runnerPromise != null)
		{
			runnerPromise.SetResult();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[DebuggerHidden]
	public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
	{
		if (runnerPromise == null)
		{
			AsyncUniTask<TStateMachine>.SetStateMachine(ref stateMachine, ref runnerPromise);
		}
		awaiter.OnCompleted(runnerPromise.MoveNext);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[DebuggerHidden]
	[SecuritySafeCritical]
	public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
	{
		if (runnerPromise == null)
		{
			AsyncUniTask<TStateMachine>.SetStateMachine(ref stateMachine, ref runnerPromise);
		}
		awaiter.UnsafeOnCompleted(runnerPromise.MoveNext);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[DebuggerHidden]
	public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
	{
		stateMachine.MoveNext();
	}

	[DebuggerHidden]
	public void SetStateMachine(IAsyncStateMachine stateMachine)
	{
	}
}
[StructLayout(LayoutKind.Auto)]
public struct AsyncUniTaskMethodBuilder<T>
{
	private IStateMachineRunnerPromise<T> runnerPromise;

	private Exception ex;

	private T result;

	public UniTask<T> Task
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[DebuggerHidden]
		get
		{
			if (runnerPromise != null)
			{
				return runnerPromise.Task;
			}
			if (ex != null)
			{
				return UniTask.FromException<T>(ex);
			}
			return UniTask.FromResult(result);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[DebuggerHidden]
	public static AsyncUniTaskMethodBuilder<T> Create()
	{
		return default(AsyncUniTaskMethodBuilder<T>);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[DebuggerHidden]
	public void SetException(Exception exception)
	{
		if (runnerPromise == null)
		{
			ex = exception;
		}
		else
		{
			runnerPromise.SetException(exception);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[DebuggerHidden]
	public void SetResult(T result)
	{
		if (runnerPromise == null)
		{
			this.result = result;
		}
		else
		{
			runnerPromise.SetResult(result);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[DebuggerHidden]
	public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
	{
		if (runnerPromise == null)
		{
			AsyncUniTask<TStateMachine, T>.SetStateMachine(ref stateMachine, ref runnerPromise);
		}
		awaiter.OnCompleted(runnerPromise.MoveNext);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[DebuggerHidden]
	[SecuritySafeCritical]
	public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
	{
		if (runnerPromise == null)
		{
			AsyncUniTask<TStateMachine, T>.SetStateMachine(ref stateMachine, ref runnerPromise);
		}
		awaiter.UnsafeOnCompleted(runnerPromise.MoveNext);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[DebuggerHidden]
	public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
	{
		stateMachine.MoveNext();
	}

	[DebuggerHidden]
	public void SetStateMachine(IAsyncStateMachine stateMachine)
	{
	}
}
