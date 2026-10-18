using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Meta.WitAi;

namespace Meta.Voice.Logging;

internal class LogSink : ILogSink
{
	private static Thread mainThread;

	private static IErrorMitigator _errorMitigator;

	private readonly string _workingDirectory = Directory.GetCurrentDirectory();

	private readonly RingDictionaryBuffer<string, CorrelationID> _messagesCache = new RingDictionaryBuffer<string, CorrelationID>(100);

	public IErrorMitigator ErrorMitigator
	{
		get
		{
			if (_errorMitigator == null)
			{
				_errorMitigator = new ErrorMitigator();
			}
			return _errorMitigator;
		}
		set
		{
			_errorMitigator = value;
		}
	}

	public ILogWriter LogWriter { get; set; }

	public LoggerOptions Options { get; set; }

	static LogSink()
	{
		ThreadUtility.CallOnMainThread(() => mainThread = Thread.CurrentThread).WrapErrors();
	}

	internal LogSink(ILogWriter logWriter, LoggerOptions options, IErrorMitigator errorMitigator = null)
	{
		LogWriter = logWriter;
		if (errorMitigator != null)
		{
			_errorMitigator = errorMitigator;
		}
		Options = options;
	}

	public void WriteEntry(LogEntry logEntry)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[" + logEntry.TimeStamp.ToShortDateString() + " " + logEntry.TimeStamp.ToShortTimeString() + "] ");
		int length = stringBuilder.Length;
		stringBuilder.Append("[VSDK] ");
		if (Options.ColorLogs)
		{
			WrapWithLogColor(stringBuilder, length, logEntry.Verbosity);
		}
		if (string.IsNullOrEmpty(logEntry.Message) && !string.IsNullOrEmpty(logEntry.Exception.Message))
		{
			logEntry.Message = logEntry.Exception.Message;
		}
		Annotate(stringBuilder, logEntry);
		string text;
		try
		{
			text = ((!string.IsNullOrEmpty(logEntry.Message) && logEntry.Parameters != null && logEntry.Parameters.Length != 0) ? string.Format(logEntry.Message, logEntry.Parameters) : logEntry.Message);
		}
		catch
		{
			text = logEntry.Message;
		}
		stringBuilder.Append(text);
		if (_messagesCache.ContainsKey(text))
		{
			IEnumerable<CorrelationID> source = _messagesCache.Extract(logEntry.Message);
			_messagesCache.Add(logEntry.Message, logEntry.CorrelationID);
			if ((string)source.First() == (string)logEntry.CorrelationID)
			{
				stringBuilder.Append($" [{logEntry.CorrelationID}]");
			}
			else
			{
				stringBuilder.Append(" [...]");
			}
		}
		else
		{
			stringBuilder.Append($" [{logEntry.CorrelationID}]");
			_messagesCache.Add(logEntry.Message, logEntry.CorrelationID);
		}
		if (logEntry.ErrorCode.HasValue && (string)logEntry.ErrorCode.Value != null && _errorMitigator != null)
		{
			stringBuilder.Append("\nMitigation: ");
			stringBuilder.Append(_errorMitigator.GetMitigation(logEntry.ErrorCode.Value));
		}
		if (logEntry.Verbosity >= Options.StackTraceLevel && logEntry.Context != null)
		{
			stringBuilder.Append("\n");
			logEntry.Context.AppendRelevantContext(stringBuilder, Options.ColorLogs);
		}
		string message = stringBuilder.ToString();
		_ = logEntry.Exception;
		logEntry.Message = message;
		SendEntryToLogWriter(logEntry);
	}

	private void SendEntryToLogWriter(LogEntry logEntry)
	{
		switch (logEntry.Verbosity)
		{
		case VLoggerVerbosity.Error:
			WriteError(logEntry.Prefix + logEntry.Message);
			break;
		case VLoggerVerbosity.Warning:
			WriteWarning(logEntry.Prefix + logEntry.Message);
			break;
		case VLoggerVerbosity.Info:
			WriteInfo(logEntry.Prefix + logEntry.Message);
			break;
		case VLoggerVerbosity.Debug:
			WriteDebug(logEntry.Prefix + logEntry.Message);
			break;
		default:
			WriteVerbose(logEntry.Prefix + logEntry.Message);
			break;
		}
	}

	private void WrapWithLogColor(StringBuilder builder, int startIndex, VLoggerVerbosity logType)
	{
	}

	private string FormatStackTrace(string stackTrace)
	{
		if (stackTrace == null)
		{
			return string.Empty;
		}
		return new Regex("at (.+) in (.*):(\\d+)").Replace(stackTrace, Evaluator);
		string Evaluator(Match match)
		{
			string value = match.Groups[1].Value;
			string text = match.Groups[2].Value.Replace(_workingDirectory, "");
			string value2 = match.Groups[3].Value;
			if (File.Exists(text))
			{
				string fileName = Path.GetFileName(text);
				return "at " + value + " in <a href=\"" + text + "\" line=\"" + value2 + "\">" + fileName + ":<b>" + value2 + "</b></a>";
			}
			return match.Value;
		}
	}

	private void Annotate(StringBuilder sb, LogEntry logEntry)
	{
		if (!Options.LinkToCallSite)
		{
			if (!string.IsNullOrEmpty(logEntry.Category))
			{
				sb.Append("[" + logEntry.Category + "] ");
			}
			return;
		}
		(string fileName, int lineNumber) callSite = logEntry.Context.GetCallSite();
		string item = callSite.fileName;
		int item2 = callSite.lineNumber;
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item);
		if (!string.IsNullOrEmpty(logEntry.Category) && !string.Equals(fileNameWithoutExtension, logEntry.Category))
		{
			sb.Append("[" + logEntry.Category + "] ");
		}
		if (!string.IsNullOrEmpty(fileNameWithoutExtension))
		{
			sb.Append("[" + fileNameWithoutExtension + ".cs");
			if (item2 > 0)
			{
				sb.Append($":{item2}");
			}
			sb.Append("]");
			sb.Append(" ");
		}
	}

	public void WriteVerbose(string message)
	{
		if (IsSafeToLog())
		{
			LogWriter.WriteVerbose(message);
			return;
		}
		ThreadUtility.CallOnMainThread(delegate
		{
			LogWriter.WriteVerbose(message);
		}).WrapErrors();
	}

	public void WriteDebug(string message)
	{
		if (IsSafeToLog())
		{
			LogWriter.WriteDebug(message);
			return;
		}
		ThreadUtility.CallOnMainThread(delegate
		{
			LogWriter.WriteDebug(message);
		}).WrapErrors();
	}

	public void WriteInfo(string message)
	{
		if (IsSafeToLog())
		{
			LogWriter.WriteInfo(message);
			return;
		}
		ThreadUtility.CallOnMainThread(delegate
		{
			LogWriter.WriteInfo(message);
		}).WrapErrors();
	}

	public void WriteWarning(string message)
	{
		if (IsSafeToLog())
		{
			LogWriter.WriteWarning(message);
			return;
		}
		ThreadUtility.CallOnMainThread(delegate
		{
			LogWriter.WriteWarning(message);
		}).WrapErrors();
	}

	public void WriteError(string message)
	{
		if (IsSafeToLog())
		{
			LogWriter.WriteError(message);
			return;
		}
		ThreadUtility.CallOnMainThread(delegate
		{
			LogWriter.WriteError(message);
		}).WrapErrors();
	}

	private (string fileName, int lineNumber) GetCallSite(StackTrace stackTrace)
	{
		for (int i = 1; i < stackTrace.FrameCount; i++)
		{
			StackFrame frame = stackTrace.GetFrame(i);
			MethodBase method = frame.GetMethod();
			if (!(method.DeclaringType == null) && !IsLoggingClass(method.DeclaringType) && !IsSystemClass(method.DeclaringType))
			{
				string item = frame.GetFileName()?.Replace('\\', '/');
				int fileLineNumber = frame.GetFileLineNumber();
				return (fileName: item, lineNumber: fileLineNumber);
			}
		}
		WriteError("Failed to get call site information.");
		return (fileName: string.Empty, lineNumber: 0);
	}

	private static bool IsLoggingClass(Type type)
	{
		if (!typeof(ICoreLogger).IsAssignableFrom(type) && !typeof(ILogWriter).IsAssignableFrom(type))
		{
			return type == typeof(VLog);
		}
		return true;
	}

	private static bool IsSystemClass(Type type)
	{
		string @namespace = type.Namespace;
		if (@namespace == null)
		{
			return false;
		}
		if (!@namespace.StartsWith("Unity") && !@namespace.StartsWith("System"))
		{
			return @namespace.StartsWith("Microsoft");
		}
		return true;
	}

	private bool IsSafeToLog()
	{
		if ((Thread.CurrentThread.ThreadState & System.Threading.ThreadState.AbortRequested & System.Threading.ThreadState.Aborted) != 0)
		{
			return Thread.CurrentThread == mainThread;
		}
		return true;
	}
}
