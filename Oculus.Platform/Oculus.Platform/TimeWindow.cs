using System.ComponentModel;

namespace Oculus.Platform;

public enum TimeWindow
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("ONE_HOUR")]
	OneHour,
	[Description("ONE_DAY")]
	OneDay,
	[Description("ONE_WEEK")]
	OneWeek,
	[Description("THIRTY_DAYS")]
	ThirtyDays,
	[Description("NINETY_DAYS")]
	NinetyDays
}
