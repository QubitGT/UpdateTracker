using System;

namespace Backtrace.Unity.Model.Breadcrumbs;

[Flags]
public enum BacktraceBreadcrumbType
{
	None = 0,
	Manual = 1,
	Log = 2,
	Navigation = 4,
	Http = 8,
	System = 0x10,
	User = 0x20,
	Configuration = 0x40
}
