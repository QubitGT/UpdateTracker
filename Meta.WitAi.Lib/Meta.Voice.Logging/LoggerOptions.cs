namespace Meta.Voice.Logging;

public class LoggerOptions
{
	public VLoggerVerbosity MinimumVerbosity;

	public VLoggerVerbosity SuppressionLevel;

	public VLoggerVerbosity StackTraceLevel;

	public bool ColorLogs;

	public bool LinkToCallSite;

	public LoggerOptions(VLoggerVerbosity minimumVerbosity, VLoggerVerbosity suppressionLevel, VLoggerVerbosity stackTraceLevel, bool colorLogs = false, bool linkToCallSite = false)
	{
		MinimumVerbosity = minimumVerbosity;
		ColorLogs = colorLogs;
		LinkToCallSite = linkToCallSite;
		SuppressionLevel = suppressionLevel;
		StackTraceLevel = stackTraceLevel;
	}

	public void CopyFrom(LoggerOptions other)
	{
		MinimumVerbosity = other.MinimumVerbosity;
		ColorLogs = other.ColorLogs;
		LinkToCallSite = other.LinkToCallSite;
		SuppressionLevel = other.SuppressionLevel;
		StackTraceLevel = other.StackTraceLevel;
	}
}
