using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Meta.Voice.Logging;
using UnityEngine;

namespace Meta.WitAi;

public static class ThreadUtility
{
	private class EarlyTask
	{
		private Task _task;

		public EarlyTask(Task task)
		{
			_task = task;
		}

		public void Start()
		{
			_task.Start(_mainThreadScheduler);
		}
	}

	private static TaskScheduler _mainThreadScheduler;

	private static Thread _mainThread;

	private static readonly ConcurrentQueue<EarlyTask> _earlyTasks = new ConcurrentQueue<EarlyTask>();

	public static bool IsMainThread()
	{
		return Thread.CurrentThread == _mainThread;
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		if (_mainThreadScheduler == null)
		{
			_mainThread = Thread.CurrentThread;
			_mainThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();
			EarlyTask result;
			while (_earlyTasks.TryDequeue(out result))
			{
				result.Start();
			}
		}
	}

	private static Task EnqueueMainThreadTask(Task task)
	{
		if (_mainThreadScheduler != null)
		{
			task.Start(_mainThreadScheduler);
			return task;
		}
		EarlyTask item = new EarlyTask(task);
		_earlyTasks.Enqueue(item);
		return task;
	}

	public static Task CallOnMainThread(Action callback)
	{
		return CallOnMainThread(null, callback);
	}

	public static Task CallOnMainThread(IVLogger logger, Action callback)
	{
		if (!IsMainThread())
		{
			return EnqueueMainThreadTask(new Task(delegate
			{
				SafeAction(logger, callback);
			}));
		}
		return Task.FromResult(SafeAction(logger, callback));
	}

	public static Task<T> CallOnMainThread<T>(Func<T> callback)
	{
		return CallOnMainThread(null, callback);
	}

	public static Task<T> CallOnMainThread<T>(IVLogger logger, Func<T> callback)
	{
		if (!IsMainThread())
		{
			return (Task<T>)EnqueueMainThreadTask(new Task<T>(() => SafeAction(logger, callback)));
		}
		return Task.FromResult(SafeAction(logger, callback));
	}

	private static bool SafeAction(IVLogger logger, Action callback)
	{
		try
		{
			callback();
			return true;
		}
		catch (Exception ex)
		{
			if (logger == null)
			{
				VLog.E(ex);
			}
			else
			{
				logger.Error("{0}\n{1}", ex.Message, ex.StackTrace);
			}
			throw;
		}
	}

	private static T SafeAction<T>(IVLogger logger, Func<T> callback)
	{
		try
		{
			return callback();
		}
		catch (Exception ex)
		{
			if (logger == null)
			{
				VLog.E(ex);
			}
			else
			{
				logger.Error(ex, "");
			}
			throw;
		}
	}

	private static async Task SafeTask(IVLogger logger, Func<Task> callback)
	{
		try
		{
			await callback();
		}
		catch (Exception exception)
		{
			logger.Error(exception, "");
			throw;
		}
	}

	private static async Task<T> SafeTask<T>(IVLogger logger, Func<Task<T>> callback)
	{
		try
		{
			return await callback();
		}
		catch (Exception exception)
		{
			logger.Error(exception, "");
			throw;
		}
	}

	public static Task BackgroundAsync(IVLogger logger, Func<Task> callback)
	{
		if (IsMainThread())
		{
			return Task.Run(() => SafeTask(logger, callback));
		}
		return SafeTask(logger, callback);
	}

	public static Task<T> BackgroundAsync<T>(IVLogger logger, Func<Task<T>> callback)
	{
		if (IsMainThread())
		{
			return Task.Run(() => SafeTask(logger, callback));
		}
		return SafeTask(logger, callback);
	}

	public static Task Background(IVLogger logger, Action callback)
	{
		if (IsMainThread())
		{
			return Task.Run(() => SafeAction(logger, callback));
		}
		return Task.FromResult(SafeAction(logger, callback));
	}

	public static IEnumerator CoroutineAwait(Func<Task> func)
	{
		Task task = func();
		while (!task.IsCompleted)
		{
			yield return null;
		}
	}

	public static IEnumerator CoroutineAwait<T>(Func<Task<T>> func, Action<T> result)
	{
		Task<T> task = func();
		while (!task.IsCompleted)
		{
			yield return null;
		}
		result(task.Result);
	}

	public static IEnumerator CoroutineAwait<T, T1>(Func<T1, Task<T>> func, T1 data, Action<T> result)
	{
		Task<T> task = func(data);
		while (!task.IsCompleted)
		{
			yield return null;
		}
		result(task.Result);
	}

	public static IEnumerator CoroutineAwait<T, T1, T2>(Func<T1, T2, Task<T>> func, T1 data1, T2 data2, Action<T> result)
	{
		Task<T> task = func(data1, data2);
		while (!task.IsCompleted)
		{
			yield return null;
		}
		result(task.Result);
	}

	public static IEnumerator CoroutineAwait<T, T1, T2, T3>(Func<T1, T2, T3, Task<T>> func, T1 data1, T2 data2, T3 data3, Action<T> result)
	{
		Task<T> task = func(data1, data2, data3);
		while (!task.IsCompleted)
		{
			yield return null;
		}
		result(task.Result);
	}
}
