using System;

namespace Fusion;

[Obsolete]
public class ConsoleLogger : TextWriterLogger
{
	public ConsoleLogger()
		: base(Console.Out, disposeWriter: false)
	{
	}

	public override void Log(LogType logType, object message, in LogContext logContext)
	{
		switch (logType)
		{
		case LogType.Info:
		case LogType.Debug:
		case LogType.Trace:
			Console.ForegroundColor = ConsoleColor.Gray;
			break;
		case LogType.Warn:
			Console.ForegroundColor = ConsoleColor.Yellow;
			break;
		case LogType.Error:
			Console.ForegroundColor = ConsoleColor.Red;
			break;
		}
		try
		{
			base.Log(logType, message, in logContext);
		}
		finally
		{
			Console.ForegroundColor = ConsoleColor.Gray;
		}
	}

	public override void LogException(Exception ex, in LogContext logContext)
	{
		Console.ForegroundColor = ConsoleColor.Red;
		try
		{
			base.LogException(ex, in logContext);
		}
		finally
		{
			Console.ForegroundColor = ConsoleColor.Gray;
		}
	}
}
