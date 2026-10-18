using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Meta.Voice.Logging;

public sealed class LoggerRegistry : ILoggerRegistry
{
	private const string EDITOR_LOG_LEVEL_KEY = "VSDK_EDITOR_LOG_LEVEL";

	private const string EDITOR_LOG_SUPPRESSION_LEVEL_KEY = "VSDK_EDITOR_LOG_SUPPRESSION_LEVEL";

	private const string EDITOR_LOG_STACKTRACE_LEVEL_KEY = "VSDK_EDITOR_LOG_STACKTRACE_LEVEL";

	private readonly Dictionary<string, IVLogger> _loggers = new Dictionary<string, IVLogger>();

	public ILogSink LogSink { get; set; }

	public IVLoggerFactory VLoggerFactory { get; set; } = new VLoggerFactory();


	public LoggerOptions Options { get; }

	public bool PoolLoggers { get; set; } = true;


	public VLoggerVerbosity LogStackTraceLevel
	{
		get
		{
			return Options.StackTraceLevel;
		}
		set
		{
			if (Options.StackTraceLevel != value)
			{
				Options.StackTraceLevel = value;
			}
		}
	}

	public VLoggerVerbosity LogSuppressionLevel
	{
		get
		{
			return Options.SuppressionLevel;
		}
		set
		{
			if (Options.SuppressionLevel != value)
			{
				Options.SuppressionLevel = value;
			}
		}
	}

	public VLoggerVerbosity EditorLogFilteringLevel
	{
		get
		{
			return Options.MinimumVerbosity;
		}
		set
		{
			if (Options.MinimumVerbosity != value)
			{
				Options.MinimumVerbosity = value;
			}
		}
	}

	public static ILoggerRegistry Instance { get; } = new LoggerRegistry();


	public IEnumerable<IVLogger> AllLoggers => _loggers.Values;

	internal LoggerRegistry()
	{
		Options = new LoggerOptions(VLoggerVerbosity.Warning, VLoggerVerbosity.Verbose, VLoggerVerbosity.Error);
		ILogWriter logWriter = new UnityLogWriter();
		LogSink = new LogSink(logWriter, Options);
	}

	public IVLogger GetLogger(LogCategory logCategory, ILogSink logSink = null)
	{
		return new LazyLogger(() => GetCoreLogger(logCategory, logSink));
	}

	public IVLogger GetLogger(string category, ILogSink logSink)
	{
		return new LazyLogger(() => GetCoreLogger(category, logSink));
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public IVLogger GetCoreLogger(LogCategory category, ILogSink logSink)
	{
		return GetCoreLogger(category.ToString(), logSink);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public IVLogger GetCoreLogger(string category, ILogSink logSink)
	{
		if (logSink == null)
		{
			logSink = LogSink;
		}
		logSink.Options = Options;
		if (PoolLoggers)
		{
			if (!_loggers.ContainsKey(category))
			{
				_loggers.Add(category, VLoggerFactory.GetLogger(category, logSink));
			}
			return _loggers[category];
		}
		return VLoggerFactory.GetLogger(category, logSink);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	private IVLogger GetCoreLogger(ILogSink logSink = null, int frameDepth = 1)
	{
		if (logSink == null)
		{
			logSink = LogSink;
		}
		StackTrace stackTrace = new StackTrace();
		string category = LogCategory.Global.ToString();
		Type type = (stackTrace.GetFrames()?.Skip(frameDepth).FirstOrDefault(IsNonLoggingFrame))?.GetMethod()?.DeclaringType;
		if (type == null)
		{
			return GetCoreLogger(category, logSink);
		}
		LogCategoryAttribute customAttribute = type.GetCustomAttribute<LogCategoryAttribute>();
		if (customAttribute == null)
		{
			return GetCoreLogger(category, logSink);
		}
		category = customAttribute.CategoryName;
		return GetCoreLogger(category, logSink);
	}

	private bool IsNonLoggingFrame(StackFrame frame)
	{
		MethodBase methodBase = frame?.GetMethod();
		if (methodBase == null || methodBase.DeclaringType == null)
		{
			return false;
		}
		if (typeof(LoggerRegistry).IsAssignableFrom(methodBase.DeclaringType) || typeof(IVLogger).IsAssignableFrom(methodBase.DeclaringType))
		{
			return false;
		}
		if (methodBase.DeclaringType.Namespace != null)
		{
			return !methodBase.DeclaringType.Namespace.StartsWith("System");
		}
		return true;
	}
}
