using System.Collections.Generic;

namespace Meta.Voice.Logging;

public interface ILoggerRegistry
{
	LoggerOptions Options { get; }

	ILogSink LogSink { get; set; }

	VLoggerVerbosity EditorLogFilteringLevel { get; set; }

	VLoggerVerbosity LogSuppressionLevel { get; set; }

	VLoggerVerbosity LogStackTraceLevel { get; set; }

	bool PoolLoggers { get; set; }

	IEnumerable<IVLogger> AllLoggers { get; }

	IVLogger GetLogger(LogCategory logCategory, ILogSink logSink = null);

	IVLogger GetLogger(string category, ILogSink logSink = null);
}
