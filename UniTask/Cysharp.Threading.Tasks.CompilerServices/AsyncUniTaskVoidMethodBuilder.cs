using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace Cysharp.Threading.Tasks.CompilerServices;

[StructLayout(LayoutKind.Auto)]
public struct AsyncUniTaskVoidMethodBuilder
{
	private IStateMachineRunner runner;

	public UniTaskVoid Task
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[DebuggerHidden]
		get
		{
			return default(UniTaskVoid);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[DebuggerHidden]
	public static AsyncUniTaskVoidMethodBuilder Create()
	{
		return default(AsyncUniTaskVoidMethodBuilder);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[DebuggerHidden]
	public void SetException(Exception exception)
	{
		if (runner != null)
		{
			runner.Return();
			runner = null;
		}
		UniTaskScheduler.PublishUnobservedTaskException(exception);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[DebuggerHidden]
	public void SetResult()
	{
		if (runner != null)
		{
			runner.Return();
			runner = null;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[DebuggerHidden]
	public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
	{
		if (runner == null)
		{
			AsyncUniTaskVoid<TStateMachine>.SetStateMachine(ref stateMachine, ref runner);
		}
		awaiter.OnCompleted(runner.MoveNext);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[DebuggerHidden]
	[SecuritySafeCritical]
	public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
	{
		if (runner == null)
		{
			AsyncUniTaskVoid<TStateMachine>.SetStateMachine(ref stateMachine, ref runner);
		}
		awaiter.UnsafeOnCompleted(runner.MoveNext);
	}

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
