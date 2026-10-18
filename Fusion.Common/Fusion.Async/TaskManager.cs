#define DEBUG
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Fusion.Async;

public static class TaskManager
{
	private static TaskFactory TaskFactory { get; set; } = Task.Factory;


	[Conditional("FUSION_UNITY")]
	public static void Setup()
	{
		if (TaskFactory == null || TaskFactory.Equals(Task.Factory))
		{
			TaskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.FromCurrentSynchronizationContext());
		}
	}

	public static Task Service(Action recurringAction, CancellationToken cancellationToken, int interval, string serviceName = null)
	{
		if (recurringAction == null)
		{
			return Task.CompletedTask;
		}
		Task<bool> result = Task.FromResult(result: true);
		return Service(delegate
		{
			recurringAction();
			return result;
		}, cancellationToken, interval, serviceName);
	}

	public static Task Service(Func<Task<bool>> recurringAction, CancellationToken cancellationToken, int interval, string serviceName = null)
	{
		Assert.Check(recurringAction != null, "Service Action should not be null");
		Assert.Check(interval > 0, "Service delay must be greated than 0");
		Assert.Check(cancellationToken != default(CancellationToken), "Service CancellationToken can't be the default");
		return TaskFactory.StartNew((Func<Task>)async delegate
		{
			InternalLogStreams.LogDebug?.Log("Starting service: " + (serviceName ?? recurringAction.Method.Name));
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					cancellationToken.ThrowIfCancellationRequested();
					await Delay(interval, cancellationToken);
					if (!(await recurringAction()))
					{
						break;
					}
				}
				catch (OperationCanceledException)
				{
					InternalLogStreams.LogDebug?.Log("Service canceled: " + (serviceName ?? recurringAction.Method.Name));
					throw;
				}
				catch (Exception e)
				{
					InternalLogStreams.LogException?.Log(e);
					break;
				}
			}
			InternalLogStreams.LogDebug?.Log("Stopping service: " + (serviceName ?? recurringAction.Method.Name));
		}, cancellationToken, TaskFactory.CreationOptions | TaskCreationOptions.LongRunning, TaskFactory.Scheduler);
	}

	public static Task Run(Func<CancellationToken, Task> action, CancellationToken cancellationToken, TaskCreationOptions options = TaskCreationOptions.None)
	{
		Assert.Check(action != null);
		Assert.Check(cancellationToken != default(CancellationToken));
		return TaskFactory.StartNew((Func<Task>)async delegate
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				await action(cancellationToken);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex2)
			{
				Exception e = ex2;
				InternalLogStreams.LogException?.Log(e);
			}
		}, cancellationToken, TaskFactory.CreationOptions | options, TaskFactory.Scheduler);
	}

	public static Task ContinueWhenAll(Task[] precedingTasks, Func<CancellationToken, Task> action, CancellationToken cancellationToken)
	{
		Assert.Check(action != null);
		Assert.Check(cancellationToken != default(CancellationToken));
		Assert.Check(precedingTasks != null);
		Assert.Check(precedingTasks.Length != 0);
		return TaskFactory.ContinueWhenAll(precedingTasks, (Func<Task[], Task>)async delegate
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				await action(cancellationToken);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex2)
			{
				Exception e = ex2;
				InternalLogStreams.LogException?.Log(e);
			}
		}, cancellationToken, TaskFactory.ContinuationOptions, TaskFactory.Scheduler);
	}

	public static async Task Delay(int delay, CancellationToken token = default(CancellationToken))
	{
		if (RuntimeUnityFlagsSetup.IsUNITY_WEBGL)
		{
			float endTime = (float)Stopwatch.GetTimestamp() + (float)Stopwatch.Frequency * ((float)delay / 1000f);
			while (!token.IsCancellationRequested && (float)Stopwatch.GetTimestamp() - endTime < 0f)
			{
				await Task.Yield();
			}
		}
		else
		{
			await Task.Delay(delay, token);
		}
	}
}
