using System;
using System.Diagnostics;
using Meta.Voice.Logging;
using UnityEngine;

namespace Meta.WitAi;

public static class VLog
{
	private static readonly ILoggerRegistry LoggerRegistry = Meta.Voice.Logging.LoggerRegistry.Instance;

	public static bool SuppressLogs { get; set; } = !Application.isEditor && !UnityEngine.Debug.isDebugBuild;


	public static void I(object log)
	{
		Log(VLoggerVerbosity.Info, null, log);
	}

	[Obsolete("Use VLogger.Info() instead")]
	public static void I(string logCategory, object log)
	{
		Log(VLoggerVerbosity.Info, logCategory, log);
	}

	public static void D(object log)
	{
		Log(VLoggerVerbosity.Debug, null, log);
	}

	[Obsolete("Use VLogger.Debug() instead")]
	public static void D(string logCategory, object log)
	{
		Log(VLoggerVerbosity.Debug, logCategory, log);
	}

	public static void W(object log, Exception e = null)
	{
		Log(VLoggerVerbosity.Warning, null, log, e);
	}

	public static void W(string logCategory, object log, Exception e = null)
	{
		Log(VLoggerVerbosity.Warning, logCategory, log, e);
	}

	public static void E(object log, Exception e = null)
	{
		Log(VLoggerVerbosity.Error, null, log, e);
	}

	public static void E(string logCategory, object log, Exception e = null)
	{
		Log(VLoggerVerbosity.Error, logCategory, log, e);
	}

	private static void Log(VLoggerVerbosity logType, string logCategory, object log, Exception exception = null)
	{
		string text = logCategory;
		if (string.IsNullOrEmpty(text))
		{
			text = GetCallingCategory();
		}
		IVLogger logger = LoggerRegistry.GetLogger(text);
		switch (logType)
		{
		case VLoggerVerbosity.Error:
			logger.Error(KnownErrorCode.Unknown, log?.ToString() + ((exception == null) ? "" : $"\n{exception}"));
			break;
		case VLoggerVerbosity.Warning:
			logger.Warning(log.ToString());
			break;
		default:
			logger.Debug(log.ToString(), null, null, null, null, "Log", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Lib\\Wit\\Runtime\\Utilities\\Logging\\VLog.cs", 197);
			break;
		}
	}

	private static string GetCallingCategory()
	{
		string text = new StackTrace()?.GetFrame(3)?.GetMethod().DeclaringType.Name;
		if (string.IsNullOrEmpty(text))
		{
			return "NoStacktrace";
		}
		return text;
	}
}
