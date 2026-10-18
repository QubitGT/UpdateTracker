using System;

namespace Oculus.Voice.Core.Utilities;

public class DateTimeUtility
{
	public static DateTime UtcNow => DateTime.UtcNow;

	public static long ElapsedMilliseconds => UtcNow.Ticks / 10000;
}
