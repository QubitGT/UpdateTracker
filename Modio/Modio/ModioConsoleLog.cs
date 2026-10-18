using System;

namespace Modio;

public class ModioConsoleLog : IModioLogHandler
{
	private readonly string _logPrefix = "[mod.io] ";

	public ModioConsoleLog()
		: this("[mod.io] ")
	{
	}

	public ModioConsoleLog(string logPrefix)
	{
		_logPrefix = logPrefix;
	}

	public void LogHandler(LogLevel logLevel, object message)
	{
		string text = logLevel switch
		{
			LogLevel.Error => "[ERROR] ", 
			LogLevel.Warning => "[WARNING] ", 
			_ => string.Empty, 
		};
		Console.WriteLine($"{_logPrefix}{text}{text}{message}");
	}
}
