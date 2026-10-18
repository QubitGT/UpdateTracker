using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace UnityEngine.ProBuilder;

internal static class Log
{
	public const string k_ProBuilderLogFileName = "ProBuilderLog.txt";

	private static Stack<LogLevel> s_logStack = new Stack<LogLevel>();

	private static LogLevel s_LogLevel = LogLevel.All;

	private static LogOutput s_Output = LogOutput.Console;

	private static string s_LogFilePath = "ProBuilderLog.txt";

	public static void PushLogLevel(LogLevel level)
	{
		s_logStack.Push(s_LogLevel);
		s_LogLevel = level;
	}

	public static void PopLogLevel()
	{
		s_LogLevel = s_logStack.Pop();
	}

	public static void SetLogLevel(LogLevel level)
	{
		s_LogLevel = level;
	}

	public static void SetOutput(LogOutput output)
	{
		s_Output = output;
	}

	public static void SetLogFile(string path)
	{
		s_LogFilePath = path;
	}

	[Conditional("DEBUG")]
	public static void Debug<T>(T value)
	{
	}

	[Conditional("DEBUG")]
	public static void Debug(string message)
	{
		DoPrint(message, LogType.Log);
	}

	[Conditional("DEBUG")]
	public static void Debug(string format, params object[] values)
	{
	}

	public static void Info(string format, params object[] values)
	{
		Info(string.Format(format, values));
	}

	public static void Info(string message)
	{
		if ((s_LogLevel & LogLevel.Info) > LogLevel.None)
		{
			DoPrint(message, LogType.Log);
		}
	}

	public static void Warning(string format, params object[] values)
	{
		Warning(string.Format(format, values));
	}

	public static void Warning(string message)
	{
		if ((s_LogLevel & LogLevel.Warning) > LogLevel.None)
		{
			DoPrint(message, LogType.Warning);
		}
	}

	public static void Error(string format, params object[] values)
	{
		Error(string.Format(format, values));
	}

	public static void Error(string message)
	{
		if ((s_LogLevel & LogLevel.Error) > LogLevel.None)
		{
			DoPrint(message, LogType.Error);
		}
	}

	[Conditional("CONSOLE_PRO_ENABLED")]
	internal static void Watch<T, K>(T key, K value)
	{
		UnityEngine.Debug.Log(string.Format("{0} : {1}\nCPAPI:{{\"cmd\":\"Watch\" \"name\":\"{0}\"}}", key.ToString(), value.ToString()));
	}

	private static void DoPrint(string message, LogType type)
	{
		if ((s_Output & LogOutput.Console) > LogOutput.None)
		{
			PrintToConsole(message, type);
		}
		if ((s_Output & LogOutput.File) > LogOutput.None)
		{
			PrintToFile(message, s_LogFilePath);
		}
	}

	private static void PrintToFile(string message, string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return;
		}
		string fullPath = Path.GetFullPath(path);
		if (string.IsNullOrEmpty(fullPath))
		{
			PrintToConsole("m_LogFilePath bad: " + fullPath);
			return;
		}
		if (!File.Exists(fullPath))
		{
			string directoryName = Path.GetDirectoryName(fullPath);
			if (string.IsNullOrEmpty(directoryName))
			{
				PrintToConsole("m_LogFilePath bad: " + fullPath);
				return;
			}
			Directory.CreateDirectory(directoryName);
			using StreamWriter streamWriter = File.CreateText(fullPath);
			streamWriter.WriteLine(message);
			return;
		}
		using StreamWriter streamWriter2 = File.AppendText(fullPath);
		streamWriter2.WriteLine(message);
	}

	public static void ClearLogFile()
	{
		if (File.Exists(s_LogFilePath))
		{
			File.Delete(s_LogFilePath);
		}
	}

	private static void PrintToConsole(string message, LogType type = LogType.Log)
	{
		switch (type)
		{
		case LogType.Log:
			UnityEngine.Debug.Log(message);
			break;
		case LogType.Warning:
			UnityEngine.Debug.LogWarning(message);
			break;
		case LogType.Error:
			UnityEngine.Debug.LogError(message);
			break;
		default:
			UnityEngine.Debug.Log(message);
			break;
		case LogType.Assert:
			break;
		}
	}

	internal static void NotNull<T>(T obj, string message)
	{
		if (obj == null)
		{
			throw new ArgumentNullException(message);
		}
	}
}
