using System;

namespace Fusion;

[Obsolete]
public interface ILogger
{
	void Log(LogType logType, object message, in LogContext logContext);

	void LogException(Exception ex, in LogContext logContext);
}
