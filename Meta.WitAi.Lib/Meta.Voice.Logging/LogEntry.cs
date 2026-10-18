using System;

namespace Meta.Voice.Logging;

public struct LogEntry : IComparable<LogEntry>
{
	public string Category { get; }

	public DateTime TimeStamp { get; }

	public string Prefix { get; set; }

	public string Message { get; set; }

	public object[] Parameters { get; }

	public CorrelationID CorrelationID { get; }

	public VLoggerVerbosity Verbosity { get; }

	public Exception Exception { get; }

	public ErrorCode? ErrorCode { get; }

	public LoggingContext Context { get; }

	public LogEntry(string category, VLoggerVerbosity verbosity, CorrelationID correlationId, LoggingContext context, string prefix, string message, params object[] parameters)
	{
		Category = category;
		TimeStamp = DateTime.UtcNow;
		Prefix = prefix;
		Message = message;
		Parameters = parameters;
		Verbosity = verbosity;
		CorrelationID = correlationId;
		Exception = null;
		ErrorCode = (ErrorCode)null;
		Context = context;
	}

	public LogEntry(string category, VLoggerVerbosity verbosity, CorrelationID correlationId, ErrorCode errorCode, Exception exception, LoggingContext context, string prefix, string message, params object[] parameters)
	{
		Category = category;
		TimeStamp = DateTime.UtcNow;
		Prefix = prefix;
		Message = message;
		Parameters = parameters;
		Verbosity = verbosity;
		CorrelationID = correlationId;
		Exception = exception;
		ErrorCode = errorCode;
		Context = context;
	}

	public LogEntry(string category, VLoggerVerbosity verbosity, CorrelationID correlationId, ErrorCode errorCode, LoggingContext context, string prefix, string message, params object[] parameters)
	{
		Category = category;
		TimeStamp = DateTime.UtcNow;
		Prefix = prefix;
		Message = message;
		Parameters = parameters;
		Verbosity = verbosity;
		CorrelationID = correlationId;
		Exception = null;
		ErrorCode = errorCode;
		Context = context;
	}

	public override string ToString()
	{
		return string.Format(Message, Parameters) + $" [{CorrelationID}]";
	}

	public int CompareTo(LogEntry other)
	{
		return TimeStamp.CompareTo(other.TimeStamp);
	}
}
