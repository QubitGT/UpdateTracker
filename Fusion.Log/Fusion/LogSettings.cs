using System;

namespace Fusion;

[Serializable]
public struct LogSettings
{
	public LogLevel Level;

	public TraceChannels TraceChannels;

	public LogSettings(LogLevel level, TraceChannels traceChannels)
	{
		Level = level;
		TraceChannels = traceChannels;
	}
}
