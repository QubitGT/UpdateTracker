using System;

namespace Fusion;

[Obsolete]
public readonly struct LogContext
{
	public readonly string Prefix;

	public readonly object Source;

	public LogContext(string prefix, object source)
	{
		Prefix = prefix;
		Source = source;
	}
}
