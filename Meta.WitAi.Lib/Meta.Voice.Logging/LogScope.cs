using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Meta.Voice.TelemetryUtilities.PerformanceTracing;

namespace Meta.Voice.Logging;

public class LogScope : ILogScope, IDisposable, ICoreLogger
{
	private readonly ICoreLogger _logger;

	private readonly int _sequenceId;

	private ConcurrentDictionary<int, string> _activeSamples = new ConcurrentDictionary<int, string>();

	public CorrelationID CorrelationID { get; set; }

	public LogScope(ICoreLogger logger, VLoggerVerbosity verbosity, CorrelationID correlationID, string message, object[] parameters)
	{
		CorrelationID = correlationID;
		_logger = logger;
		_sequenceId = _logger.Start(correlationID, verbosity, message, parameters);
	}

	public void Verbose(string message, params object[] parameters)
	{
		_logger.Log(CorrelationID, VLoggerVerbosity.Verbose, message, parameters);
	}

	public void Verbose(CorrelationID correlationId, string message, params object[] parameters)
	{
		Correlate(correlationId, CorrelationID);
		_logger.Log(correlationId, VLoggerVerbosity.Verbose, message, parameters);
	}

	public void Verbose(string message, object p1 = null, object p2 = null, object p3 = null, object p4 = null, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		_logger.Verbose(message, p1, p2, p3, p4, memberName, sourceFilePath, sourceLineNumber);
	}

	public void Info(string message, params object[] parameters)
	{
		_logger.Log(CorrelationID, VLoggerVerbosity.Info, message, parameters);
	}

	public void Info(CorrelationID correlationId, string message, params object[] parameters)
	{
		Correlate(correlationId, CorrelationID);
		_logger.Log(correlationId, VLoggerVerbosity.Info, message, parameters);
	}

	public void Info(string message, object p1 = null, object p2 = null, object p3 = null, object p4 = null, string memberName = "", string sourceFilePath = "", int sourceLineNumber = 0)
	{
		_logger.Info(message, p1, p2, p3, p4, memberName, sourceFilePath, sourceLineNumber);
	}

	public void Debug(string message, params object[] parameters)
	{
		_logger.Log(CorrelationID, VLoggerVerbosity.Debug, message, parameters);
	}

	public void Debug(string message, object p1 = null, object p2 = null, object p3 = null, object p4 = null, string memberName = "", string sourceFilePath = "", int sourceLineNumber = 0)
	{
		_logger.Debug(message, p1, p2, p3, p4, memberName, sourceFilePath, sourceLineNumber);
	}

	public void Debug(CorrelationID correlationId, string message, params object[] parameters)
	{
		Correlate(correlationId, CorrelationID);
		_logger.Log(correlationId, VLoggerVerbosity.Debug, message, parameters);
	}

	public void Warning(string message, params object[] parameters)
	{
		_logger.Log(CorrelationID, VLoggerVerbosity.Warning, message, parameters);
	}

	public void Warning(CorrelationID correlationId, string message, params object[] parameters)
	{
		Correlate(correlationId, CorrelationID);
		_logger.Log(correlationId, VLoggerVerbosity.Warning, message, parameters);
	}

	public void Error(ErrorCode errorCode, string message, params object[] parameters)
	{
		_logger.Log(CorrelationID, VLoggerVerbosity.Error, message, errorCode, parameters);
	}

	public void Error(CorrelationID correlationId, ErrorCode errorCode, string message, params object[] parameters)
	{
		Correlate(correlationId, CorrelationID);
		_logger.Log(correlationId, VLoggerVerbosity.Error, message, errorCode, parameters);
	}

	public void Error(CorrelationID correlationId, Exception exception, string message, params object[] parameters)
	{
		Correlate(correlationId, CorrelationID);
		_logger.Log(correlationId, VLoggerVerbosity.Error, exception, KnownErrorCode.Unknown, message, parameters);
	}

	public void Error(Exception exception, ErrorCode errorCode, string message, params object[] parameters)
	{
		_logger.Log(CorrelationID, VLoggerVerbosity.Verbose, exception, errorCode, message, parameters);
	}

	public void Error(Exception exception, string message = "", params object[] parameters)
	{
		_logger.Log(CorrelationID, VLoggerVerbosity.Error, exception, KnownErrorCode.Unknown, "");
	}

	public void Error(CorrelationID correlationId, Exception exception, ErrorCode errorCode, string message, params object[] parameters)
	{
		Correlate(correlationId, CorrelationID);
		_logger.Log(correlationId, VLoggerVerbosity.Verbose, exception, errorCode, message, parameters);
	}

	public void Error(CorrelationID correlationId, string message, params object[] parameters)
	{
		_logger.Log(correlationId, VLoggerVerbosity.Error, message, parameters);
	}

	public void Error(string message, params object[] parameters)
	{
		_logger.Log(CorrelationID, VLoggerVerbosity.Error, message, parameters);
	}

	public int Start(CorrelationID correlationId, VLoggerVerbosity verbosity, string message, params object[] parameters)
	{
		Correlate(correlationId, CorrelationID);
		int num = _logger.Start(correlationId, verbosity, message, parameters);
		StartProfiling(num, message);
		return num;
	}

	public int Start(VLoggerVerbosity verbosity, string message, params object[] parameters)
	{
		VsdkProfiler.BeginSample(message);
		int num = _logger.Start(verbosity, message, parameters);
		StartProfiling(num, message);
		return num;
	}

	private void StartProfiling(int sequenceId, string message)
	{
		if (VsdkProfiler.profilingEnabled)
		{
			VsdkProfiler.BeginSample(message);
			_activeSamples[sequenceId] = message;
		}
	}

	public void End(int sequenceId)
	{
		if (VsdkProfiler.profilingEnabled && _activeSamples.TryRemove(sequenceId, out var value))
		{
			VsdkProfiler.EndSample(value);
		}
		_logger.End(sequenceId);
	}

	public void Correlate(CorrelationID newCorrelationId, CorrelationID rootCorrelationId)
	{
		_logger.Correlate(newCorrelationId, rootCorrelationId);
	}

	public void Log(CorrelationID correlationId, VLoggerVerbosity verbosity, string message, params object[] parameters)
	{
		_logger.Log(correlationId, verbosity, message, parameters);
	}

	public void Log(CorrelationID correlationId, VLoggerVerbosity verbosity, Exception exception, ErrorCode errorCode, string message, params object[] parameters)
	{
		_logger.Log(correlationId, verbosity, exception, errorCode, message, parameters);
	}

	public void Log(CorrelationID correlationId, VLoggerVerbosity verbosity, ErrorCode errorCode, string message, params object[] parameters)
	{
		_logger.Log(correlationId, verbosity, errorCode, message, parameters);
	}

	public void Dispose()
	{
		_logger.End(_sequenceId);
	}
}
