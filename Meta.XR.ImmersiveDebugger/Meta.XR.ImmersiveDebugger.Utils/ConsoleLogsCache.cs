using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.Utils;

internal static class ConsoleLogsCache
{
	internal static Action<string, string, LogType> OnLogReceived;

	private static readonly List<(string, string, LogType)> StartupLogs = new List<(string, string, LogType)>();

	private static SynchronizationContext _mainThreadContext;

	private static void OnApplicationQuitting()
	{
		Application.logMessageReceivedThreaded -= EnqueueLogEntry;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void OnLoad()
	{
		StartupLogs?.Clear();
		_mainThreadContext = null;
		Application.quitting -= OnApplicationQuitting;
		Application.logMessageReceivedThreaded -= EnqueueLogEntry;
		StartCachingLogs();
	}

	private static void StartCachingLogs()
	{
		_mainThreadContext = SynchronizationContext.Current;
		if (RuntimeSettings.Instance.ImmersiveDebuggerEnabled)
		{
			Application.logMessageReceivedThreaded += EnqueueLogEntry;
			Application.quitting += OnApplicationQuitting;
		}
	}

	internal static void ConsumeStartupLogs(Action<string, string, LogType> logProcessor)
	{
		foreach (var startupLog in StartupLogs)
		{
			logProcessor(startupLog.Item1, startupLog.Item2, startupLog.Item3);
		}
		StartupLogs.Clear();
	}

	private static void EnqueueLogEntry(string logString, string stackTrace, LogType type)
	{
		_mainThreadContext.Post(delegate
		{
			if (OnLogReceived == null)
			{
				StartupLogs.Add((logString, stackTrace, type));
			}
			else
			{
				OnLogReceived(logString, stackTrace, type);
			}
		}, null);
	}
}
