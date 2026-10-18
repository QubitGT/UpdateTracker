using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using UnityEngine;

namespace Fusion;

internal sealed class UnityLogStream : LogStream
{
	private readonly FusionUnityLoggerBase _logger;

	private readonly LogLevel _logLevel;

	private readonly string _prefix;

	private readonly LogFlags _flags;

	public UnityLogStream(FusionUnityLoggerBase logger, LogLevel logLevel, TraceChannels channel, LogFlags flags)
	{
		_prefix = ((channel == (TraceChannels)0 || channel == TraceChannels.Global) ? "" : channel.ToString());
		_logLevel = logLevel;
		_logger = logger;
		_flags = flags;
	}

	public override void Log(ILogSource source, string message)
	{
		FusionUnityLoggerBase logger = _logger;
		FusionUnityLoggerBase.LogContext context = new FusionUnityLoggerBase.LogContext(message, _prefix, source, _flags);
		var (text, context2) = logger.CreateMessage(in context);
		if (text != null)
		{
			switch (_logLevel)
			{
			case LogLevel.Error:
				Debug.LogError(text, context2);
				break;
			case LogLevel.Warn:
				Debug.LogWarning(text, context2);
				break;
			default:
				Debug.Log(text, context2);
				break;
			}
		}
	}

	public override void Log(string message)
	{
		FusionUnityLoggerBase logger = _logger;
		FusionUnityLoggerBase.LogContext context = new FusionUnityLoggerBase.LogContext(message, _prefix, null, _flags);
		var (text, context2) = logger.CreateMessage(in context);
		if (text != null)
		{
			switch (_logLevel)
			{
			case LogLevel.Error:
				Debug.LogError(text, context2);
				break;
			case LogLevel.Warn:
				Debug.LogWarning(text, context2);
				break;
			default:
				Debug.Log(text, context2);
				break;
			}
		}
	}

	public override void Log(ILogSource source, string message, Exception error)
	{
		FusionUnityLoggerBase logger = _logger;
		FusionUnityLoggerBase.LogContext context = new FusionUnityLoggerBase.LogContext((message ?? error.GetType().FullName) + " <i>See next error log entry for details.</i>", null, source, (LogFlags)0);
		var (text, @object) = logger.CreateMessage(in context);
		if (text == null)
		{
			return;
		}
		Debug.LogWarning(text, @object);
		if (Application.isEditor)
		{
			ExceptionDispatchInfo edi = ExceptionDispatchInfo.Capture(error);
			Thread thread = new Thread((ThreadStart)delegate
			{
				edi.Throw();
			});
			thread.Start();
			thread.Join();
		}
		else if ((bool)@object)
		{
			Debug.LogException(error, @object);
		}
		else
		{
			Debug.LogException(error);
		}
	}

	public override void Log(string message, Exception error)
	{
		FusionUnityLoggerBase logger = _logger;
		FusionUnityLoggerBase.LogContext context = new FusionUnityLoggerBase.LogContext((message ?? error.GetType().FullName) + " <i>See next error log entry for details.</i>", null, null, (LogFlags)0);
		var (text, @object) = logger.CreateMessage(in context);
		if (text == null)
		{
			return;
		}
		Debug.LogWarning(text, @object);
		if (Application.isEditor)
		{
			ExceptionDispatchInfo edi = ExceptionDispatchInfo.Capture(error);
			Thread thread = new Thread((ThreadStart)delegate
			{
				edi.Throw();
			});
			thread.Start();
			thread.Join();
		}
		else if ((bool)@object)
		{
			Debug.LogException(error, @object);
		}
		else
		{
			Debug.LogException(error);
		}
	}

	public override void Log(ILogSource source, Exception error)
	{
		FusionUnityLoggerBase logger = _logger;
		FusionUnityLoggerBase.LogContext context = new FusionUnityLoggerBase.LogContext(error.GetType().FullName + " <i>See next error log entry for details.</i>", null, source, (LogFlags)0);
		var (text, @object) = logger.CreateMessage(in context);
		if (text == null)
		{
			return;
		}
		Debug.LogWarning(text, @object);
		if (Application.isEditor)
		{
			ExceptionDispatchInfo edi = ExceptionDispatchInfo.Capture(error);
			Thread thread = new Thread((ThreadStart)delegate
			{
				edi.Throw();
			});
			thread.Start();
			thread.Join();
		}
		else if ((bool)@object)
		{
			Debug.LogException(error, @object);
		}
		else
		{
			Debug.LogException(error);
		}
	}

	public override void Log(Exception error)
	{
		FusionUnityLoggerBase logger = _logger;
		FusionUnityLoggerBase.LogContext context = new FusionUnityLoggerBase.LogContext(error.GetType().FullName + " <i>See next error log entry for details.</i>", null, null, (LogFlags)0);
		var (text, @object) = logger.CreateMessage(in context);
		if (text == null)
		{
			return;
		}
		Debug.LogWarning(text, @object);
		if (Application.isEditor)
		{
			ExceptionDispatchInfo edi = ExceptionDispatchInfo.Capture(error);
			Thread thread = new Thread((ThreadStart)delegate
			{
				edi.Throw();
			});
			thread.Start();
			thread.Join();
		}
		else if ((bool)@object)
		{
			Debug.LogException(error, @object);
		}
		else
		{
			Debug.LogException(error);
		}
	}
}
