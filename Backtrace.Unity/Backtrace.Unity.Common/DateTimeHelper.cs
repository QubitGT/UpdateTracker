using System;

namespace Backtrace.Unity.Common;

internal static class DateTimeHelper
{
	private static TimeSpan Now()
	{
		return DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1));
	}

	public static int Timestamp()
	{
		return (int)Now().TotalSeconds;
	}

	public static double TimestampMs()
	{
		return Now().TotalMilliseconds;
	}
}
