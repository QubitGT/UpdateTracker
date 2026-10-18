using System;

namespace Modio;

public class ModioLog
{
	public delegate void LogHandler(LogLevel logLevel, object message);

	public const string LOG_PREFIX_DEFAULT = "[mod.io] ";

	private static IModioLogHandler _logHandler;

	private readonly LogLevel _logLevel;

	public static ModioLog? Error { get; private set; }

	public static ModioLog? Warning { get; private set; }

	public static ModioLog? Message { get; private set; }

	public static ModioLog? Verbose { get; private set; }

	static ModioLog()
	{
		ApplyLogLevel(LogLevel.Verbose);
		ModioServices.Bind<IModioLogHandler>().FromNew<ModioConsoleLog>(ModioServicePriority.Default);
		ModioServices.AddBindingChangedListener<IModioLogHandler>(UpdateLogHandler);
		if (ModioCommandLine.TryGet("loglevel", out var value))
		{
			if (Enum.TryParse<LogLevel>(value, ignoreCase: true, out var result))
			{
				ApplyLogLevel(result);
			}
			else
			{
				Error?.Log("Unrecognized log level: " + value);
			}
		}
		else
		{
			ModioServices.AddBindingChangedListener<ModioSettings>(GetLogLevelFromSettings);
		}
	}

	private static void UpdateLogHandler(IModioLogHandler logHandler)
	{
		_logHandler = logHandler;
	}

	private static void GetLogLevelFromSettings(ModioSettings settings)
	{
		ApplyLogLevel(settings.LogLevel);
	}

	private static void ApplyLogLevel(LogLevel logLevel)
	{
		Error = (((int)logLevel < 1) ? null : (Error ?? new ModioLog(LogLevel.Error)));
		Warning = (((int)logLevel < 2) ? null : (Warning ?? new ModioLog(LogLevel.Warning)));
		Message = (((int)logLevel < 3) ? null : (Message ?? new ModioLog(LogLevel.Message)));
		Verbose = (((int)logLevel < 4) ? null : (Verbose ?? new ModioLog(LogLevel.Verbose)));
	}

	private ModioLog(LogLevel logLevel)
	{
		_logLevel = logLevel;
	}

	public void Log(object message)
	{
		if (_logHandler != null)
		{
			_logHandler.LogHandler(_logLevel, message);
		}
		else
		{
			Console.WriteLine(string.Format("{0}{1}: {2}", "[mod.io] ", _logLevel, message));
		}
	}

	public static ModioLog? GetLogLevel(LogLevel logLevel)
	{
		return logLevel switch
		{
			LogLevel.Error => Error, 
			LogLevel.Warning => Warning, 
			LogLevel.Message => Message, 
			LogLevel.Verbose => Verbose, 
			_ => Error, 
		};
	}
}
