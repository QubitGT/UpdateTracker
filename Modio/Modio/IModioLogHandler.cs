namespace Modio;

public interface IModioLogHandler
{
	void LogHandler(LogLevel logLevel, object message);
}
