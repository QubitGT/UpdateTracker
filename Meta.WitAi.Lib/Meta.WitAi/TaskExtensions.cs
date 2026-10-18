using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Meta.WitAi;

public static class TaskExtensions
{
	public static void WrapErrors(this Task task)
	{
		task.ContinueWith(delegate(Task t, object state)
		{
			if (t.Exception != null)
			{
				VLog.E(t.Exception);
			}
		}, null);
	}

	public static async Task<bool> TimeoutAfter(this Task task, int ms)
	{
		bool timedOut = false;
		if (task != await Task.WhenAny(task, Task.Delay(ms)))
		{
			timedOut = true;
		}
		else if (task.Exception != null)
		{
			VLog.E((object)("Task threw an exception while waiting for timeout: " + task.Exception.Message), (Exception)task.Exception);
			throw task.Exception;
		}
		return !timedOut;
	}

	public static Task WhenLessThan(this ICollection<Task> tasks, int max)
	{
		return tasks.WhenLessThan(max, CancellationToken.None);
	}

	public static Task WhenLessThan(this ICollection<Task> tasks, int max, CancellationToken cancellationToken)
	{
		if (tasks == null)
		{
			throw new ArgumentNullException("tasks");
		}
		TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();
		cancellationToken.Register(delegate
		{
			completion.TrySetCanceled();
		});
		int running = tasks.Count;
		max = Mathf.Max(0, max);
		foreach (Task task in tasks)
		{
			if (task.IsCompleted)
			{
				running--;
				continue;
			}
			task.ContinueWith(delegate
			{
				if (!completion.Task.IsCompleted && Interlocked.Decrement(ref running) < max)
				{
					completion.SetResult(result: true);
				}
			}, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
		}
		if (!completion.Task.IsCompleted && running < max)
		{
			completion.SetResult(result: true);
		}
		return completion.Task;
	}
}
