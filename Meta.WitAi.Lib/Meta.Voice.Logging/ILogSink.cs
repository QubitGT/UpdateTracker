namespace Meta.Voice.Logging;

public interface ILogSink
{
	IErrorMitigator ErrorMitigator { get; set; }

	LoggerOptions Options { get; set; }

	ILogWriter LogWriter { get; set; }

	void WriteEntry(LogEntry logEntry);

	void WriteError(string message);
}
