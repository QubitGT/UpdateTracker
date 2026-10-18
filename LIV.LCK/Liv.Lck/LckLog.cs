using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Liv.Lck.Core;
using Liv.Lck.Settings;
using UnityEngine;

namespace Liv.Lck;

internal static class LckLog
{
	private static readonly Queue<(Liv.Lck.Core.LogType type, string message, string memberName, string filePath, int lineNumber)> _earlyLogs = new Queue<(Liv.Lck.Core.LogType, string, string, string, int)>();

	private static bool _isInitialized = false;

	private static readonly object _lockObject = new object();

	internal static void OnLckCoreInitialized()
	{
		lock (_lockObject)
		{
			_isInitialized = true;
			while (_earlyLogs.Count > 0)
			{
				var (level, message, memberName, filePath, lineNumber) = _earlyLogs.Dequeue();
				LckCore.Log(level, message, memberName, filePath, lineNumber);
			}
		}
	}

	public static void Log(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
	{
		if (ShouldPrint(LogLevel.Info))
		{
			UnityEngine.Debug.Log(message);
		}
		SendToLckCore(Liv.Lck.Core.LogType.Info, message, memberName, GetFileName(filePath), lineNumber);
	}

	public static void LogWarning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
	{
		if (ShouldPrint(LogLevel.Warning))
		{
			UnityEngine.Debug.LogWarning(message);
		}
		SendToLckCore(Liv.Lck.Core.LogType.Warning, message, memberName, GetFileName(filePath), lineNumber);
	}

	public static void LogError(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
	{
		if (ShouldPrint(LogLevel.Error))
		{
			UnityEngine.Debug.LogError(message);
		}
		SendToLckCore(Liv.Lck.Core.LogType.Error, message, memberName, GetFileName(filePath), lineNumber);
	}

	[Conditional("LCK_TRACE")]
	public static void LogTrace(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
	{
		UnityEngine.Debug.Log(message);
		SendToLckCore(Liv.Lck.Core.LogType.Trace, message, memberName, GetFileName(filePath), lineNumber);
	}

	private static void SendToLckCore(Liv.Lck.Core.LogType type, string message, string memberName, string filePath, int lineNumber)
	{
		lock (_lockObject)
		{
			if (_isInitialized)
			{
				LckCore.Log(type, message, memberName, filePath, lineNumber);
			}
			else
			{
				_earlyLogs.Enqueue((type, message, memberName, filePath, lineNumber));
			}
		}
	}

	private static bool ShouldPrint(LogLevel level)
	{
		return LckSettings.Instance.BaseLogLevel >= level;
	}

	private static string GetFileName(string filePath)
	{
		int num = filePath.LastIndexOfAny(new char[2] { '/', '\\' });
		if (num >= 0 && num < filePath.Length - 1)
		{
			return filePath.Substring(num + 1);
		}
		return filePath;
	}
}
