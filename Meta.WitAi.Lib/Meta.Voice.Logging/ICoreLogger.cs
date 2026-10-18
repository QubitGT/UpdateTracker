using System;
using System.Runtime.CompilerServices;

namespace Meta.Voice.Logging;

public interface ICoreLogger
{
	CorrelationID CorrelationID { get; set; }

	void Verbose(string message, params object[] parameters);

	void Verbose(CorrelationID correlationId, string message, params object[] parameters);

	void Verbose(string message, object p1 = null, object p2 = null, object p3 = null, object p4 = null, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0);

	void Info(string message, params object[] parameters);

	void Info(CorrelationID correlationId, string message, params object[] parameters);

	void Info(string message, object p1 = null, object p2 = null, object p3 = null, object p4 = null, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0);

	void Debug(string message, params object[] parameters);

	void Debug(string message, object p1 = null, object p2 = null, object p3 = null, object p4 = null, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0);

	void Debug(CorrelationID correlationId, string message, params object[] parameters);

	void Warning(CorrelationID correlationId, string message, params object[] parameters);

	void Warning(string message, params object[] parameters);

	void Error(CorrelationID correlationId, ErrorCode errorCode, string message, params object[] parameters);

	void Error(ErrorCode errorCode, string message, params object[] parameters);

	void Error(CorrelationID correlationId, Exception exception, ErrorCode errorCode, string message, params object[] parameters);

	void Error(CorrelationID correlationId, string message, params object[] parameters);

	void Error(string message, params object[] parameters);

	void Error(CorrelationID correlationId, Exception exception, string message = "", params object[] parameters);

	void Error(Exception exception, ErrorCode errorCode, string message = "", params object[] parameters);

	void Error(Exception exception, string message = "", params object[] parameters);

	ILogScope Scope(VLoggerVerbosity verbosity, string message, params object[] parameters)
	{
		return new LogScope(this, verbosity, CorrelationID, message, parameters);
	}

	ILogScope Scope(CorrelationID correlationId, VLoggerVerbosity verbosity, string message, params object[] parameters)
	{
		Correlate(correlationId, CorrelationID);
		return new LogScope(this, verbosity, correlationId, message, parameters);
	}

	int Start(CorrelationID correlationId, VLoggerVerbosity verbosity, string message, params object[] parameters);

	int Start(VLoggerVerbosity verbosity, string message, params object[] parameters);

	void End(int sequenceId);

	void Correlate(CorrelationID newCorrelationId, CorrelationID rootCorrelationId);

	void Log(CorrelationID correlationId, VLoggerVerbosity verbosity, string message, params object[] parameters);

	void Log(CorrelationID correlationId, VLoggerVerbosity verbosity, Exception exception, ErrorCode errorCode, string message = "", params object[] parameters);

	void Log(CorrelationID correlationId, VLoggerVerbosity verbosity, ErrorCode errorCode, string message, params object[] parameters);
}
