using System;

namespace Fusion;

[Obsolete("Use LogLevel instead")]
public enum LogType : byte
{
	Error,
	Warn,
	Info,
	Debug,
	Trace
}
