using System;
using System.Collections.Generic;
using UnityEngine;

namespace Backtrace.Unity.Model.Breadcrumbs;

internal sealed class BacktraceBreadcrumbs : IBacktraceBreadcrumbs
{
	internal readonly IBacktraceLogManager LogManager;

	internal readonly BacktraceBreadcrumbsEventHandler EventHandler;

	private bool _enabled;

	public BacktraceBreadcrumbType BreadcrumbsLevel { get; internal set; }

	public UnityEngineLogLevel UnityLogLevel { get; set; }

	public BacktraceBreadcrumbs(IBacktraceLogManager logManager, BacktraceBreadcrumbType level, UnityEngineLogLevel unityLogLevel)
	{
		BreadcrumbsLevel = level;
		UnityLogLevel = unityLogLevel;
		LogManager = logManager;
		EventHandler = new BacktraceBreadcrumbsEventHandler(this);
	}

	public void UnregisterEvents()
	{
		EventHandler.Unregister();
	}

	public bool ClearBreadcrumbs()
	{
		return LogManager.Clear();
	}

	public bool EnableBreadcrumbs(BacktraceBreadcrumbType level, UnityEngineLogLevel unityLogLevel)
	{
		UnityLogLevel = unityLogLevel;
		BreadcrumbsLevel = level;
		return EnableBreadcrumbs();
	}

	public bool EnableBreadcrumbs()
	{
		if (_enabled)
		{
			return false;
		}
		if (!LogManager.Enable())
		{
			return false;
		}
		EventHandler.Register(BreadcrumbsLevel);
		return true;
	}

	public bool FromBacktrace(BacktraceReport report)
	{
		UnityEngineLogLevel type = (report.ExceptionTypeReport ? UnityEngineLogLevel.Error : UnityEngineLogLevel.Info);
		if (!ShouldLog(BreadcrumbLevel.System, type))
		{
			return false;
		}
		return AddBreadcrumbs(report.Message, BreadcrumbLevel.System, type);
	}

	public bool FromMonoBehavior(string message, LogType type, IDictionary<string, string> attributes)
	{
		return AddBreadcrumbs(message, BreadcrumbLevel.System, ConvertLogTypeToLogLevel(type), attributes);
	}

	public string GetBreadcrumbLogPath()
	{
		return LogManager.BreadcrumbsFilePath;
	}

	public bool Info(string message)
	{
		return Log(message, LogType.Log, null);
	}

	public bool Info(string message, IDictionary<string, string> attributes)
	{
		return Log(message, LogType.Log, attributes);
	}

	public bool Warning(string message)
	{
		return Log(message, LogType.Warning, null);
	}

	public bool Warning(string message, IDictionary<string, string> attributes)
	{
		return Log(message, LogType.Warning, attributes);
	}

	public bool Debug(string message, IDictionary<string, string> attributes)
	{
		return Log(message, LogType.Assert, attributes);
	}

	public bool Debug(string message)
	{
		return Log(message, LogType.Assert);
	}

	public bool Exception(string message)
	{
		return Log(message, LogType.Exception, null);
	}

	public bool Exception(Exception exception, IDictionary<string, string> attributes)
	{
		return Log(exception.Message, LogType.Exception, attributes);
	}

	public bool Exception(Exception exception)
	{
		return Log(exception.Message, LogType.Exception, null);
	}

	public bool Exception(string message, IDictionary<string, string> attributes)
	{
		return Log(message, LogType.Exception, attributes);
	}

	public bool Log(string message, LogType type)
	{
		return Log(message, type, null);
	}

	public bool Log(string message, LogType logType, IDictionary<string, string> attributes)
	{
		return Log(message, BreadcrumbLevel.Manual, logType, attributes);
	}

	public bool Log(string message, BreadcrumbLevel level, LogType logType, IDictionary<string, string> attributes)
	{
		UnityEngineLogLevel type = ConvertLogTypeToLogLevel(logType);
		return AddBreadcrumbs(message, level, type, attributes);
	}

	internal bool AddBreadcrumbs(string message, BreadcrumbLevel level, UnityEngineLogLevel type, IDictionary<string, string> attributes = null)
	{
		if (!ShouldLog(level, type))
		{
			return false;
		}
		return LogManager.Add(message, level, type, attributes);
	}

	internal bool ShouldLog(BreadcrumbLevel level, UnityEngineLogLevel type)
	{
		return ShouldLog((BacktraceBreadcrumbType)level, type);
	}

	internal bool ShouldLog(BacktraceBreadcrumbType level, UnityEngineLogLevel type)
	{
		if (!BreadcrumbsLevel.HasFlag(level))
		{
			return false;
		}
		return UnityLogLevel.HasFlag(type);
	}

	internal static UnityEngineLogLevel ConvertLogTypeToLogLevel(LogType type)
	{
		switch (type)
		{
		case LogType.Warning:
			return UnityEngineLogLevel.Warning;
		case LogType.Error:
		case LogType.Exception:
			return UnityEngineLogLevel.Error;
		case LogType.Assert:
			return UnityEngineLogLevel.Debug;
		default:
			return UnityEngineLogLevel.Info;
		}
	}

	public double BreadcrumbId()
	{
		return LogManager.BreadcrumbId();
	}

	public void Update()
	{
		EventHandler.Update();
	}

	public static bool CanStoreBreadcrumbs(UnityEngineLogLevel logLevel, BacktraceBreadcrumbType backtraceBreadcrumbsLevel)
	{
		if (backtraceBreadcrumbsLevel != 0)
		{
			return logLevel != UnityEngineLogLevel.None;
		}
		return false;
	}

	public string Archive()
	{
		if (!(LogManager is IArchiveableBreadcrumbManager archiveableBreadcrumbManager))
		{
			return string.Empty;
		}
		return archiveableBreadcrumbManager.Archive();
	}
}
