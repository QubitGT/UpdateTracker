using System;

namespace Backtrace.Unity.Model.Breadcrumbs;

[Flags]
public enum UnityEngineLogLevel
{
	None = 0,
	Debug = 1,
	Warning = 2,
	Info = 4,
	Fatal = 8,
	Error = 0x10
}
