using System;
using System.Diagnostics;
using System.Globalization;

namespace Backtrace.Unity.Common;

internal static class MetricsHelper
{
	public static string GetMicroseconds(this Stopwatch stopwatch)
	{
		return Math.Max(1L, stopwatch.ElapsedTicks * 1000000 / Stopwatch.Frequency).ToString(CultureInfo.InvariantCulture);
	}

	public static void Restart(this Stopwatch stopwatch)
	{
		stopwatch.Stop();
		stopwatch.Reset();
		stopwatch.Start();
	}
}
