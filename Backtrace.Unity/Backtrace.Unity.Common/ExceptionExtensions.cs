using System;
using Backtrace.Unity.Model;

namespace Backtrace.Unity.Common;

public static class ExceptionExtensions
{
	public static BacktraceReport ToBacktraceReport(this Exception source)
	{
		return new BacktraceReport(source);
	}
}
