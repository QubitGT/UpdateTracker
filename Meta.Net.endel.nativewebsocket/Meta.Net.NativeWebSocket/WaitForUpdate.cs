using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Meta.Net.NativeWebSocket;

public class WaitForUpdate : CustomYieldInstruction
{
	public class MainThreadAwaiter : INotifyCompletion
	{
		private Action continuation;

		public bool IsCompleted { get; set; }

		void INotifyCompletion.OnCompleted(Action continuation)
		{
			this.continuation = continuation;
		}

		public void GetResult()
		{
		}

		public void Complete()
		{
			IsCompleted = true;
			continuation?.Invoke();
		}
	}

	public override bool keepWaiting => false;

	public MainThreadAwaiter GetAwaiter()
	{
		MainThreadAwaiter mainThreadAwaiter = new MainThreadAwaiter();
		MainThreadUtil.Run(CoroutineWrapper(this, mainThreadAwaiter));
		return mainThreadAwaiter;
	}

	public static IEnumerator CoroutineWrapper(IEnumerator theWorker, MainThreadAwaiter awaiter)
	{
		yield return theWorker;
		awaiter.Complete();
	}
}
