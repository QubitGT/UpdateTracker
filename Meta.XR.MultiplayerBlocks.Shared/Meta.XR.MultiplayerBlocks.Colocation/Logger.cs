using System;
using UnityEngine;

namespace Meta.XR.MultiplayerBlocks.Colocation;

internal static class Logger
{
	private static bool _isVerboseLogVisible;

	private static bool _isInfoLogVisible;

	private static bool _isWarningLogVisible;

	private static bool _isErrorLogVisible;

	private static bool _isSharedSpatialAnchorsErrorVisible;

	public static void Log(string message, LogLevel logLevel)
	{
		switch (logLevel)
		{
		case LogLevel.Verbose:
			LogVerbose(message);
			break;
		case LogLevel.Info:
			LogInfo(message);
			break;
		case LogLevel.Warning:
			LogWarning(message);
			break;
		case LogLevel.Error:
			LogError(message);
			break;
		case LogLevel.SharedSpatialAnchorsError:
			LogSharedSpatialAnchorsError(message);
			break;
		default:
			throw new ArgumentOutOfRangeException("logLevel", logLevel, $"colocationLogLevel is unknown: {logLevel}");
		}
	}

	private static void LogVerbose(string message)
	{
		if (_isVerboseLogVisible)
		{
			Debug.Log(GetPrefixMessage(LogLevel.Verbose) + message);
		}
	}

	private static void LogInfo(string message)
	{
		if (_isInfoLogVisible)
		{
			Debug.Log(GetPrefixMessage(LogLevel.Info) + message);
		}
	}

	private static void LogWarning(string message)
	{
		if (_isWarningLogVisible)
		{
			Debug.LogWarning(GetPrefixMessage(LogLevel.Warning) + message);
		}
	}

	private static void LogError(string message)
	{
		if (_isErrorLogVisible)
		{
			Debug.LogError(GetPrefixMessage(LogLevel.Error) + message);
		}
	}

	private static void LogSharedSpatialAnchorsError(string message)
	{
		if (_isSharedSpatialAnchorsErrorVisible)
		{
			Debug.LogError(GetPrefixMessage(LogLevel.SharedSpatialAnchorsError) + message);
		}
	}

	private static string GetPrefixMessage(LogLevel logLevel)
	{
		return $"[{logLevel}] ";
	}

	public static void SetLogLevelVisibility(LogLevel logLevel, bool value)
	{
		switch (logLevel)
		{
		case LogLevel.Verbose:
			_isVerboseLogVisible = value;
			break;
		case LogLevel.Info:
			_isInfoLogVisible = value;
			break;
		case LogLevel.Warning:
			_isWarningLogVisible = value;
			break;
		case LogLevel.Error:
			_isErrorLogVisible = value;
			break;
		case LogLevel.SharedSpatialAnchorsError:
			_isSharedSpatialAnchorsErrorVisible = value;
			break;
		default:
			throw new ArgumentOutOfRangeException("logLevel", logLevel, $"colocationLogLevel is unknown: {logLevel}");
		}
	}

	public static void SetAllLogsVisibility(bool value)
	{
		_isVerboseLogVisible = value;
		_isInfoLogVisible = value;
		_isWarningLogVisible = value;
		_isErrorLogVisible = value;
		_isSharedSpatialAnchorsErrorVisible = value;
	}
}
