using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Fusion;

public static class Log
{
	public delegate LogStream CreateLogStreamDelegate(LogLevel level, LogFlags flags, TraceChannels channel);

	private readonly ref struct Factory
	{
		public readonly CreateLogStreamDelegate StreamFactory;

		public readonly LogSettings Settings;

		public Factory(LogSettings settings, CreateLogStreamDelegate streamFactory)
		{
			StreamFactory = streamFactory;
			Settings = settings;
		}

		public void Init(ref DebugLogStream stream, TraceChannels channel)
		{
			DisposeAndNullify(ref stream);
			if (StreamFactory != null && LogLevel.Debug >= Settings.Level)
			{
				LogStream logStream = StreamFactory(LogLevel.Info, LogFlags.Debug, channel);
				LogStream logStream2 = StreamFactory(LogLevel.Warn, LogFlags.Debug, channel);
				LogStream logStream3 = StreamFactory(LogLevel.Error, LogFlags.Debug, channel);
				if (logStream == null && logStream2 == null && logStream3 == null)
				{
					stream = null;
				}
				else
				{
					stream = new DebugLogStream(logStream, logStream2, logStream3);
				}
			}
			else
			{
				stream = null;
			}
		}

		public void Init(ref TraceLogStream stream, TraceChannels channel)
		{
			DisposeAndNullify(ref stream);
			if (StreamFactory != null && Settings.TraceChannels.HasFlag(channel))
			{
				LogStream logStream = StreamFactory(LogLevel.Info, LogFlags.Trace, channel);
				LogStream logStream2 = StreamFactory(LogLevel.Warn, LogFlags.Trace, channel);
				LogStream logStream3 = StreamFactory(LogLevel.Error, LogFlags.Trace, channel);
				if (logStream == null && logStream2 == null && logStream3 == null)
				{
					stream = null;
				}
				else
				{
					stream = new TraceLogStream(logStream, logStream2, logStream3);
				}
			}
			else
			{
				stream = null;
			}
		}

		public void Init(ref LogStream stream, LogLevel logLevel)
		{
			DisposeAndNullify(ref stream);
			if (StreamFactory != null && logLevel >= Settings.Level)
			{
				stream = StreamFactory(logLevel, (LogFlags)0, (TraceChannels)0);
			}
			else
			{
				stream = null;
			}
		}
	}

	private class DelegateMessageStream : LogStream
	{
		private readonly Action<string> _logAction;

		private readonly Action<Exception> _exceptionAction;

		private readonly string _prefix;

		public DelegateMessageStream(Action<string> action, Action<Exception> exceptionAction, string prefix = null)
		{
			_logAction = action ?? throw new ArgumentNullException("action");
			_exceptionAction = exceptionAction ?? throw new ArgumentNullException("exceptionAction");
			_prefix = prefix;
		}

		public override void Log(ILogSource source, string message)
		{
			Log(message);
		}

		public override void Log(string message)
		{
			if (!string.IsNullOrEmpty(_prefix))
			{
				_logAction(_prefix + " " + message);
			}
			else
			{
				_logAction(message);
			}
		}

		public override void Log(ILogSource source, string message, Exception error)
		{
			_exceptionAction(error);
		}

		public override void Log(string message, Exception error)
		{
			_exceptionAction(error);
		}

		public override void Log(Exception error)
		{
			_exceptionAction(error);
		}
	}

	public static bool IsInitialized { get; private set; }

	public static LogSettings Settings { get; private set; }

	[Obsolete("Use IsInitialized instead")]
	public static bool Initialized => IsInitialized;

	public static void Dispose()
	{
		Factory factory = new Factory(default(LogSettings), null);
		InitInternal(in factory);
		IsInitialized = false;
	}

	public static void Initialize(LogLevel logLevel, CreateLogStreamDelegate streamFactory, TraceChannels traceChannels = (TraceChannels)0)
	{
		Factory factory = new Factory(new LogSettings(logLevel, traceChannels), streamFactory);
		InitInternal(in factory);
	}

	public static void Initialize(LogSettings settings, CreateLogStreamDelegate streamFactory)
	{
		Factory factory = new Factory(settings, streamFactory);
		InitInternal(in factory);
	}

	public static void InitializeForConsole(LogSettings settings)
	{
		Factory factory = new Factory(settings, (LogLevel type, LogFlags flags, TraceChannels chanel) => new ConsoleLogStream(flags.HasFlag(LogFlags.Debug) ? ConsoleColor.DarkGray : ConsoleColor.Gray, flags.HasFlag(LogFlags.Debug) ? "[DEBUG] " : ""));
		InitInternal(in factory);
	}

	private static void DisposeAndNullify<T>(ref T obj) where T : class, IDisposable
	{
		obj?.Dispose();
		obj = null;
	}

	private static void InitPartial(in Factory factory)
	{
		factory.Init(ref InternalLogStreams.LogTrace, TraceChannels.Global);
		factory.Init(ref InternalLogStreams.LogTraceStun, TraceChannels.Stun);
		factory.Init(ref InternalLogStreams.LogTraceObject, TraceChannels.Object);
		factory.Init(ref InternalLogStreams.LogTraceNetwork, TraceChannels.Network);
		factory.Init(ref InternalLogStreams.LogTracePrefab, TraceChannels.Prefab);
		factory.Init(ref InternalLogStreams.LogTraceSceneInfo, TraceChannels.SceneInfo);
		factory.Init(ref InternalLogStreams.LogTraceSceneManager, TraceChannels.SceneManager);
		factory.Init(ref InternalLogStreams.LogTraceSimulationMessage, TraceChannels.SimulationMessage);
		factory.Init(ref InternalLogStreams.LogTraceHostMigration, TraceChannels.HostMigration);
		factory.Init(ref InternalLogStreams.LogTraceEncryption, TraceChannels.Encryption);
		factory.Init(ref InternalLogStreams.LogTraceDummyTraffic, TraceChannels.DummyTraffic);
		factory.Init(ref InternalLogStreams.LogTraceRealtime, TraceChannels.Realtime);
		factory.Init(ref InternalLogStreams.LogTraceMemoryTrack, TraceChannels.MemoryTrack);
		factory.Init(ref InternalLogStreams.LogTraceSnapshots, TraceChannels.Snapshots);
		factory.Init(ref InternalLogStreams.LogTraceTime, TraceChannels.Time);
	}

	private static void InitInternal(in Factory factory)
	{
		factory.Init(ref InternalLogStreams.LogDebug, TraceChannels.Global);
		factory.Init(ref InternalLogStreams.LogInfo, LogLevel.Info);
		factory.Init(ref InternalLogStreams.LogWarn, LogLevel.Warn);
		factory.Init(ref InternalLogStreams.LogError, LogLevel.Error);
		factory.Init(ref InternalLogStreams.LogException, LogLevel.Error);
		InitPartial(in factory);
		Settings = factory.Settings;
		IsInitialized = true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_LOGLEVEL_TRACE")]
	[Conditional("FUSION_LOGLEVEL_DEBUG")]
	public static void Debug(string message)
	{
		InternalLogStreams.LogDebug?.InfoStream.Log(message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_LOGLEVEL_TRACE")]
	[Conditional("FUSION_LOGLEVEL_DEBUG")]
	public static void DebugWarn(string message)
	{
		InternalLogStreams.LogDebug?.WarnStream.Log(message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_LOGLEVEL_TRACE")]
	[Conditional("FUSION_LOGLEVEL_DEBUG")]
	public static void DebugError(string message)
	{
		InternalLogStreams.LogDebug?.ErrorStream.Log(message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_LOGLEVEL_TRACE")]
	[Conditional("FUSION_LOGLEVEL_DEBUG")]
	[Conditional("FUSION_LOGLEVEL_INFO")]
	public static void Info(string message)
	{
		InternalLogStreams.LogInfo?.Log(message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_LOGLEVEL_TRACE")]
	[Conditional("FUSION_LOGLEVEL_DEBUG")]
	[Conditional("FUSION_LOGLEVEL_INFO")]
	public static void Info(ILogSource logSource, string message)
	{
		InternalLogStreams.LogInfo?.Log(logSource, message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_LOGLEVEL_TRACE")]
	[Conditional("FUSION_LOGLEVEL_DEBUG")]
	[Conditional("FUSION_LOGLEVEL_INFO")]
	[Conditional("FUSION_LOGLEVEL_WARN")]
	public static void Warn(string message)
	{
		InternalLogStreams.LogWarn?.Log(message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_LOGLEVEL_TRACE")]
	[Conditional("FUSION_LOGLEVEL_DEBUG")]
	[Conditional("FUSION_LOGLEVEL_INFO")]
	[Conditional("FUSION_LOGLEVEL_WARN")]
	public static void Warn(ILogSource logSource, string message)
	{
		InternalLogStreams.LogWarn?.Log(logSource, message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_LOGLEVEL_TRACE")]
	[Conditional("FUSION_LOGLEVEL_DEBUG")]
	[Conditional("FUSION_LOGLEVEL_INFO")]
	[Conditional("FUSION_LOGLEVEL_WARN")]
	[Conditional("FUSION_LOGLEVEL_ERROR")]
	public static void Error(string message)
	{
		InternalLogStreams.LogError?.Log(message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_LOGLEVEL_TRACE")]
	[Conditional("FUSION_LOGLEVEL_DEBUG")]
	[Conditional("FUSION_LOGLEVEL_INFO")]
	[Conditional("FUSION_LOGLEVEL_WARN")]
	[Conditional("FUSION_LOGLEVEL_ERROR")]
	public static void Error(ILogSource logSource, string message)
	{
		InternalLogStreams.LogError?.Log(logSource, message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_LOGLEVEL_TRACE")]
	[Conditional("FUSION_LOGLEVEL_DEBUG")]
	[Conditional("FUSION_LOGLEVEL_INFO")]
	[Conditional("FUSION_LOGLEVEL_WARN")]
	[Conditional("FUSION_LOGLEVEL_ERROR")]
	public static void Exception(Exception ex)
	{
		InternalLogStreams.LogException?.Log(ex);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_LOGLEVEL_TRACE")]
	[Conditional("FUSION_LOGLEVEL_DEBUG")]
	[Conditional("FUSION_LOGLEVEL_INFO")]
	[Conditional("FUSION_LOGLEVEL_WARN")]
	[Conditional("FUSION_LOGLEVEL_ERROR")]
	public static void Exception(string message, Exception ex)
	{
		InternalLogStreams.LogException?.Log(message, ex);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_LOGLEVEL_TRACE")]
	[Conditional("FUSION_LOGLEVEL_DEBUG")]
	[Conditional("FUSION_LOGLEVEL_INFO")]
	[Conditional("FUSION_LOGLEVEL_WARN")]
	[Conditional("FUSION_LOGLEVEL_ERROR")]
	public static void Exception(ILogSource source, string message, Exception ex)
	{
		InternalLogStreams.LogException?.Log(source, message, ex);
	}

	[Obsolete("Use InitializeForConsole instead")]
	public static void InitForConsole()
	{
		InitForConsole(LogType.Info);
	}

	[Obsolete("Use InitializeForConsole instead")]
	public static void InitForConsole(LogType logType)
	{
		LogLevel level = logType switch
		{
			LogType.Error => LogLevel.Error, 
			LogType.Warn => LogLevel.Warn, 
			LogType.Debug => LogLevel.Info, 
			LogType.Trace => LogLevel.Info, 
			LogType.Info => LogLevel.Info, 
			_ => throw new ArgumentOutOfRangeException("logType", logType, null), 
		};
		switch (logType)
		{
		case LogType.Debug:
		{
			bool flag = true;
			break;
		}
		case LogType.Trace:
		{
			bool flag = true;
			break;
		}
		default:
		{
			bool flag = false;
			break;
		}
		}
		LogSettings settings = default(LogSettings);
		settings.Level = level;
		settings.TraceChannels = (TraceChannels)0;
		InitializeForConsole(settings);
	}

	[Obsolete("Use Initialize instead")]
	public static void Init(Action<string> info, Action<string> warn, Action<string> error, Action<Exception> exn)
	{
		LogLevel level = LogLevel.Error;
		if (warn != null)
		{
			level = LogLevel.Warn;
			if (info != null)
			{
				level = LogLevel.Info;
			}
		}
		LogSettings settings = default(LogSettings);
		settings.Level = level;
		settings.TraceChannels = (TraceChannels)0;
		Initialize(settings, (LogLevel type, LogFlags flags, TraceChannels name) => type switch
		{
			LogLevel.Info => new DelegateMessageStream(info, exn), 
			LogLevel.Warn => new DelegateMessageStream(warn, exn), 
			LogLevel.Error => new DelegateMessageStream(error, exn), 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		});
	}

	[Conditional("TRACE")]
	[Obsolete("Use string overloads instead")]
	public static void Trace(object msg)
	{
	}

	[Conditional("TRACE")]
	[Obsolete("Use string overloads instead")]
	public static void TraceWarn(object msg)
	{
	}

	[Conditional("TRACE")]
	[Obsolete("Use string overloads instead")]
	public static void TraceError(object msg)
	{
	}

	[Conditional("TRACE")]
	[Obsolete("Use string overloads instead")]
	public static void Trace<T>(T source, object msg) where T : ILogSource
	{
	}

	[Conditional("TRACE")]
	[Obsolete("Use string overloads instead")]
	public static void TraceWarn<T>(T source, object msg) where T : ILogSource
	{
	}

	[Conditional("TRACE")]
	[Obsolete("Use string overloads instead")]
	public static void TraceError<T>(T source, object msg) where T : ILogSource
	{
	}

	[Conditional("DEBUG")]
	[Obsolete("Use string overloads instead")]
	public static void Debug(object msg)
	{
	}

	[Conditional("DEBUG")]
	[Obsolete("Use string overloads instead")]
	public static void DebugWarn(object msg)
	{
	}

	[Conditional("DEBUG")]
	[Obsolete("Use string overloads instead")]
	public static void DebugError(object msg)
	{
	}

	[Conditional("DEBUG")]
	[Obsolete("Use string overloads instead")]
	public static void Debug<T>(T source, object msg) where T : ILogSource
	{
	}

	[Conditional("DEBUG")]
	[Obsolete("Use string overloads instead")]
	public static void DebugWarn<T>(T source, object msg) where T : ILogSource
	{
	}

	[Conditional("DEBUG")]
	[Obsolete("Use string overloads instead")]
	public static void DebugError<T>(T source, object msg) where T : ILogSource
	{
	}

	[Obsolete("Use overloads with strings instead")]
	public static void Info(object msg)
	{
	}

	[Obsolete("Use overloads with strings instead")]
	internal static void Info(ILogSource source, object msg)
	{
	}

	[Obsolete("Use overloads with strings instead")]
	public static void Warn(object msg)
	{
	}

	[Obsolete("Use overloads with strings instead")]
	internal static void Warn(ILogSource source, object msg)
	{
	}

	[Obsolete("Use overloads with strings instead")]
	public static void Error(object msg)
	{
	}

	[Obsolete("Use overloads with strings instead")]
	internal static void Error(ILogSource source, object msg)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_TRACE_GLOBAL")]
	public static void Trace(string msg)
	{
		InternalLogStreams.LogTrace?.InfoStream.Log(msg);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_TRACE_GLOBAL")]
	public static void TraceWarn(string msg)
	{
		InternalLogStreams.LogTrace?.WarnStream.Log(msg);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_TRACE_GLOBAL")]
	public static void TraceError(string msg)
	{
		InternalLogStreams.LogTrace?.ErrorStream.Log(msg);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_TRACE_GLOBAL")]
	public static void Trace<T>(T source, string msg) where T : ILogSource
	{
		InternalLogStreams.LogTrace?.InfoStream.Log(source, msg);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_TRACE_GLOBAL")]
	public static void TraceWarn<T>(T source, string msg) where T : ILogSource
	{
		InternalLogStreams.LogTrace?.WarnStream.Log(source, msg);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_TRACE_GLOBAL")]
	public static void TraceError<T>(T source, string msg) where T : ILogSource
	{
		InternalLogStreams.LogTrace?.ErrorStream.Log(source, msg);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_TRACE_SCENEMANAGER")]
	public static void TraceSceneManager(string msg)
	{
		InternalLogStreams.LogTraceSceneManager?.InfoStream.Log(msg);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_TRACE_SCENEMANAGER")]
	public static void TraceSceneManagerWarn(string msg)
	{
		InternalLogStreams.LogTraceSceneManager?.WarnStream.Log(msg);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_TRACE_SCENEMANAGER")]
	public static void TraceSceneManagerError(string msg)
	{
		InternalLogStreams.LogTraceSceneManager?.ErrorStream.Log(msg);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_TRACE_SCENEMANAGER")]
	public static void TraceSceneManager<T>(T source, string msg) where T : ILogSource
	{
		InternalLogStreams.LogTraceSceneManager?.InfoStream.Log(source, msg);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_TRACE_SCENEMANAGER")]
	public static void TraceSceneManagerWarn<T>(T source, string msg) where T : ILogSource
	{
		InternalLogStreams.LogTraceSceneManager?.WarnStream.Log(source, msg);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_TRACE_SCENEMANAGER")]
	public static void TraceSceneManagerError<T>(T source, string msg) where T : ILogSource
	{
		InternalLogStreams.LogTraceSceneManager?.ErrorStream.Log(source, msg);
	}
}
