using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Meta.WitAi;

public static class TaskUtility
{
	public static Task FromAsyncResult(IAsyncResult asyncResult)
	{
		if (asyncResult.IsCompleted)
		{
			return Task.FromResult(result: true);
		}
		return Task.Factory.FromAsync(asyncResult, StubForTaskFactory);
	}

	private static void StubForTaskFactory(IAsyncResult result)
	{
	}

	public static Task FromAsyncOp(AsyncOperation asyncOperation)
	{
		if (asyncOperation.isDone)
		{
			return Task.FromResult(result: true);
		}
		TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();
		asyncOperation.completed += delegate
		{
			completion.SetResult(result: true);
		};
		return completion.Task;
	}

	public static async Task WaitForTimeout(int timeoutMs, Func<DateTime> getLastUpdate = null, Task completionTask = null)
	{
		int num = timeoutMs;
		while (num > 0)
		{
			if (completionTask != null)
			{
				if ((await Task.WhenAny(completionTask, Task.Delay(num))).Equals(completionTask))
				{
					break;
				}
			}
			else
			{
				await Task.Delay(num);
			}
			DateTime utcNow = DateTime.UtcNow;
			DateTime dateTime = getLastUpdate?.Invoke() ?? utcNow;
			double totalMilliseconds = (utcNow - dateTime).TotalMilliseconds;
			num = Mathf.Max(0, timeoutMs - (int)totalMilliseconds);
		}
	}
}
