using UnityEngine;

namespace Modio.Unity;

public class ModioUnityLogger : IModioLogHandler
{
	private readonly string _prefix;

	public ModioUnityLogger()
		: this("[mod.io] ")
	{
	}

	public ModioUnityLogger(string prefix)
	{
		_prefix = prefix;
	}

	public void LogHandler(LogLevel logLevel, object message)
	{
		string arg = logLevel switch
		{
			LogLevel.Error => "[ERROR] ", 
			LogLevel.Warning => "[WARNING] ", 
			_ => string.Empty, 
		};
		ILogger unityLogger = Debug.unityLogger;
		unityLogger.Log(logLevel switch
		{
			LogLevel.None => LogType.Log, 
			LogLevel.Error => LogType.Error, 
			LogLevel.Warning => LogType.Warning, 
			LogLevel.Message => LogType.Log, 
			LogLevel.Verbose => LogType.Log, 
			_ => LogType.Log, 
		}, $"{_prefix}{arg}{message}");
	}
}
