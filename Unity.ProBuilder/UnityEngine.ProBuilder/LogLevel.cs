using System;

namespace UnityEngine.ProBuilder;

[Flags]
internal enum LogLevel
{
	None = 0,
	Error = 1,
	Warning = 2,
	Info = 4,
	Default = 3,
	All = 0xFF
}
