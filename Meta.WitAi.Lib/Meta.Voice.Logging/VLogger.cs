using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Meta.WitAi;

namespace Meta.Voice.Logging;

internal class VLogger : IVLogger, ICoreLogger
{
	private LoggingContext _emptyContext = new LoggingContext((StackTrace)null);

	private int _nextSequenceId = 1;

	private readonly Dictionary<int, LogEntry> _scopeEntries = new Dictionary<int, LogEntry>();

	private static readonly ThreadLocal<string> CorrelationIDThreadLocal = new ThreadLocal<string>();

	private static readonly RingDictionaryBuffer<CorrelationID, LogEntry> LogBuffer = new RingDictionaryBuffer<CorrelationID, LogEntry>(1000);

	private readonly RingDictionaryBuffer<CorrelationID, CorrelationID> _correlations = new RingDictionaryBuffer<CorrelationID, CorrelationID>(100);

	private readonly RingDictionaryBuffer<CorrelationID, CorrelationID> _downStreamCorrelations = new RingDictionaryBuffer<CorrelationID, CorrelationID>(100);

	private readonly ILogSink _logSink;

	private readonly string _category;

	private CorrelationID _correlationID;

	public CorrelationID CorrelationID
	{
		get
		{
			if (_correlationID.IsAssigned)
			{
				return _correlationID;
			}
			if (!CorrelationIDThreadLocal.IsValueCreated)
			{
				CorrelationIDThreadLocal.Value = Guid.NewGuid().ToString();
			}
			_correlationID = (CorrelationID)CorrelationIDThreadLocal.Value;
			return _correlationID;
		}
		set
		{
			_correlationID = value;
			CorrelationIDThreadLocal.Value = _correlationID;
		}
	}

	internal VLogger(string category, ILogSink logSink)
	{
		_category = category;
		_logSink = logSink;
	}

	public static void ClearBuffer()
	{
		LogBuffer.Clear();
	}

	private void CorrelateIds(CorrelationID correlationId)
	{
		if (!_correlationID.IsAssigned)
		{
			CorrelationID = correlationId;
		}
		if ((string)CorrelationID != (string)correlationId && !_correlations.ContainsKey(correlationId))
		{
			Correlate(correlationId, CorrelationID);
		}
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Verbose(string message, object p1, object p2 = null, object p3 = null, object p4 = null, string memberName = "", string sourceFilePath = "", int sourceLineNumber = 0)
	{
		LoggingContext context = new LoggingContext(memberName, sourceFilePath, sourceLineNumber);
		LogEntry(new LogEntry(_category, VLoggerVerbosity.Verbose, CorrelationID, context, string.Empty, message, p1, p2, p3, p4));
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Verbose(string message, params object[] parameters)
	{
		LoggingContext emptyContext = _emptyContext;
		LogEntry(new LogEntry(_category, VLoggerVerbosity.Verbose, CorrelationID, emptyContext, string.Empty, message, parameters));
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Verbose(CorrelationID correlationId, string message, params object[] parameters)
	{
		LoggingContext emptyContext = _emptyContext;
		CorrelateIds(correlationId);
		LogEntry(new LogEntry(_category, VLoggerVerbosity.Verbose, correlationId, emptyContext, string.Empty, message, parameters));
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Info(string message, params object[] parameters)
	{
		Log(CorrelationID, VLoggerVerbosity.Info, message, parameters);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Info(CorrelationID correlationId, string message, params object[] parameters)
	{
		CorrelateIds(correlationId);
		Log(correlationId, VLoggerVerbosity.Info, message, parameters);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Info(string message, object p1, object p2 = null, object p3 = null, object p4 = null, string memberName = "", string sourceFilePath = "", int sourceLineNumber = 0)
	{
		LoggingContext context = new LoggingContext(memberName, sourceFilePath, sourceLineNumber);
		LogEntry(new LogEntry(_category, VLoggerVerbosity.Info, CorrelationID, context, string.Empty, message, p1, p2, p3, p4));
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Debug(string message, params object[] parameters)
	{
		Log(CorrelationID, VLoggerVerbosity.Debug, message, parameters);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Debug(string message, object p1, object p2 = null, object p3 = null, object p4 = null, string memberName = "", string sourceFilePath = "", int sourceLineNumber = 0)
	{
		LoggingContext context = new LoggingContext(memberName, sourceFilePath, sourceLineNumber);
		LogEntry(new LogEntry(_category, VLoggerVerbosity.Debug, CorrelationID, context, string.Empty, message, p1, p2, p3, p4));
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Debug(CorrelationID correlationId, string message, params object[] parameters)
	{
		CorrelateIds(correlationId);
		Log(correlationId, VLoggerVerbosity.Debug, message, parameters);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Warning(string message, params object[] parameters)
	{
		Log(CorrelationID, VLoggerVerbosity.Warning, message, parameters);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Error(CorrelationID correlationId, ErrorCode errorCode, string message, params object[] parameters)
	{
		CorrelateIds(correlationId);
		Log(correlationId, VLoggerVerbosity.Error, errorCode, message, parameters);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Error(ErrorCode errorCode, string message, params object[] parameters)
	{
		Log(CorrelationID, VLoggerVerbosity.Error, errorCode, message, parameters);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Error(CorrelationID correlationId, Exception exception, ErrorCode errorCode, string message, params object[] parameters)
	{
		CorrelateIds(correlationId);
		Log(correlationId, VLoggerVerbosity.Error, errorCode, exception, message, parameters);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Error(CorrelationID correlationId, string message, params object[] parameters)
	{
		CorrelateIds(correlationId);
		Log(correlationId, VLoggerVerbosity.Error, message, parameters);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Error(string message, params object[] parameters)
	{
		Log(CorrelationID, VLoggerVerbosity.Error, message, parameters);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Error(CorrelationID correlationId, Exception exception, string message, params object[] parameters)
	{
		CorrelateIds(correlationId);
		Log(correlationId, VLoggerVerbosity.Error, (ErrorCode)KnownErrorCode.Unknown, exception, message, parameters);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Error(Exception exception, ErrorCode errorCode, string message, params object[] parameters)
	{
		Log(CorrelationID, VLoggerVerbosity.Error, errorCode, exception, message, parameters);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Error(Exception exception, string message = "", params object[] parameters)
	{
		Log(CorrelationID, VLoggerVerbosity.Error, exception, KnownErrorCode.Unknown, message, parameters);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Warning(CorrelationID correlationId, string message, params object[] parameters)
	{
		CorrelateIds(correlationId);
		Log(correlationId, VLoggerVerbosity.Warning, message, parameters);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Correlate(CorrelationID newCorrelationId, CorrelationID rootCorrelationId)
	{
		if (_correlations.Add(newCorrelationId, rootCorrelationId, unique: true) | _downStreamCorrelations.Add(rootCorrelationId, newCorrelationId, unique: true))
		{
			Log(newCorrelationId, VLoggerVerbosity.Verbose, "Correlated: {0}", newCorrelationId);
		}
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Log(CorrelationID correlationId, VLoggerVerbosity verbosity, string message, params object[] parameters)
	{
		LoggingContext context = new LoggingContext(new StackTrace(fNeedFileInfo: true));
		LogEntry(new LogEntry(_category, verbosity, correlationId, context, string.Empty, message, parameters));
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Log(CorrelationID correlationId, VLoggerVerbosity verbosity, Exception exception, ErrorCode errorCode, string message, params object[] parameters)
	{
		LoggingContext context = new LoggingContext(new StackTrace(fNeedFileInfo: true));
		LogEntry(new LogEntry(_category, verbosity, correlationId, errorCode, exception, context, string.Empty, message, parameters));
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Log(CorrelationID correlationId, VLoggerVerbosity verbosity, ErrorCode errorCode, string message, params object[] parameters)
	{
		LoggingContext context = new LoggingContext(new StackTrace(fNeedFileInfo: true));
		LogEntry(new LogEntry(_category, verbosity, correlationId, errorCode, context, string.Empty, message, parameters));
	}

	private void LogEntry(LogEntry logEntry)
	{
		if (IsSuppressed(logEntry))
		{
			LogBuffer.Add(logEntry.CorrelationID, logEntry);
		}
		else if (IsFiltered(logEntry))
		{
			LogBuffer.Add(logEntry.CorrelationID, logEntry);
		}
		else
		{
			Write(logEntry);
		}
	}

	private bool IsFiltered(LogEntry logEntry)
	{
		return false;
	}

	private bool IsSuppressed(LogEntry logEntry)
	{
		if (VLog.SuppressLogs && logEntry.Verbosity < VLoggerVerbosity.Error)
		{
			return true;
		}
		return false;
	}

	public ILogScope Scope(VLoggerVerbosity verbosity, string message, params object[] parameters)
	{
		return new LogScope(this, verbosity, CorrelationID, message, parameters);
	}

	public ILogScope Scope(CorrelationID correlationId, VLoggerVerbosity verbosity, string message, params object[] parameters)
	{
		return new LogScope(this, verbosity, correlationId, message, parameters);
	}

	public int Start(CorrelationID correlationId, VLoggerVerbosity verbosity, string message, params object[] parameters)
	{
		CorrelateIds(correlationId);
		LoggingContext context = new LoggingContext(new StackTrace(fNeedFileInfo: true));
		LogEntry logEntry = new LogEntry(_category, verbosity, correlationId, context, "Started: ", message, parameters);
		LogBuffer.Add(correlationId, logEntry);
		_scopeEntries.Add(_nextSequenceId, logEntry);
		if (!IsFiltered(logEntry))
		{
			Write(logEntry);
		}
		return _nextSequenceId++;
	}

	public int Start(VLoggerVerbosity verbosity, string message, params object[] parameters)
	{
		LoggingContext context = new LoggingContext(new StackTrace(fNeedFileInfo: true));
		LogEntry logEntry = new LogEntry(_category, verbosity, CorrelationID, context, "Started: ", message, parameters);
		LogBuffer.Add(CorrelationID, logEntry);
		_scopeEntries.Add(_nextSequenceId, logEntry);
		if (!IsFiltered(logEntry))
		{
			Write(logEntry);
		}
		return _nextSequenceId++;
	}

	public void End(int sequenceId)
	{
		if (!_scopeEntries.ContainsKey(sequenceId))
		{
			Error(KnownErrorCode.Logging, "Attempted to end a scope that was not started. Scope ID: {0}", sequenceId);
			return;
		}
		LogEntry logEntry = _scopeEntries[sequenceId];
		if (!IsFiltered(logEntry))
		{
			logEntry.Prefix = "Finished: ";
			Write(logEntry);
		}
		_scopeEntries.Remove(sequenceId);
	}

	public void Flush(CorrelationID correlationID)
	{
		List<LogEntry> list = ExtractRelatedEntries(correlationID);
		list.Sort();
		foreach (LogEntry item in list)
		{
			Write(item, force: true);
		}
	}

	private List<LogEntry> ExtractRelatedEntries(CorrelationID correlationID)
	{
		List<LogEntry> entries = new List<LogEntry>();
		CorrelationID key = correlationID;
		entries.AddRange(LogBuffer.Extract(key));
		while (_correlations.ContainsKey(key))
		{
			if (_correlations[key].Count > 1)
			{
				Warning(correlationID, KnownErrorCode.Logging, "Correlation ID {0} had multiple parent IDs. Found: {1} IDs.", correlationID, _correlations[key].Count);
			}
			key = _correlations[key].First();
			entries.AddRange(LogBuffer.Extract(key));
		}
		ExtractDownstreamRelatedEntries(correlationID, ref entries);
		return entries;
	}

	private void ExtractDownstreamRelatedEntries(CorrelationID correlationID, ref List<LogEntry> entries)
	{
		if (!_downStreamCorrelations.ContainsKey(correlationID))
		{
			return;
		}
		foreach (CorrelationID item in _downStreamCorrelations[correlationID])
		{
			entries.AddRange(LogBuffer.Extract(item));
			ExtractDownstreamRelatedEntries(item, ref entries);
		}
	}

	public void Flush()
	{
		foreach (LogEntry item in LogBuffer.ExtractAll())
		{
			Write(item, force: true);
		}
	}

	public IEnumerable<LogEntry> ExtractAllEntries()
	{
		return LogBuffer.ExtractAll();
	}

	private void Write(LogEntry logEntry, bool force = false)
	{
		if (logEntry.Verbosity == VLoggerVerbosity.Error)
		{
			Flush(logEntry.CorrelationID);
		}
		if (!(!force & IsSuppressed(logEntry)))
		{
			_logSink.WriteEntry(logEntry);
		}
	}

	internal string GetDependenciesStructure(CorrelationID? correlationID = null, int depth = 0)
	{
		CorrelationID correlationID2 = correlationID ?? CorrelationID;
		string text = new string(' ', depth * 2) + correlationID2;
		if (_downStreamCorrelations.ContainsKey(correlationID2))
		{
			foreach (CorrelationID item in _downStreamCorrelations[correlationID2])
			{
				text = text + "\n" + GetDependenciesStructure(item, depth + 1);
			}
		}
		return text;
	}
}
