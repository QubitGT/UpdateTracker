using System.ComponentModel;

namespace Oculus.Platform;

public enum LeaderboardFilterType
{
	[Description("NONE")]
	None,
	[Description("FRIENDS")]
	Friends,
	[Description("UNKNOWN")]
	Unknown,
	[Description("USER_IDS")]
	UserIds
}
