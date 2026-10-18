using System;

namespace Modio.Extensions;

public static class DateTimeExtensions
{
	public static DateTime GetUtcDateTime(this long timeStamp)
	{
		return DateTime.UnixEpoch.AddSeconds(timeStamp);
	}
}
