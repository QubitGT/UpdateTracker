using System.ComponentModel;

namespace Oculus.Platform;

public enum AchievementType
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("SIMPLE")]
	Simple,
	[Description("BITFIELD")]
	Bitfield,
	[Description("COUNT")]
	Count
}
