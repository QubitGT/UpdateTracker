using System.ComponentModel;

namespace Oculus.Platform;

public enum LaunchType
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("NORMAL")]
	Normal,
	[Description("INVITE")]
	Invite,
	[Description("COORDINATED")]
	Coordinated,
	[Description("DEEPLINK")]
	Deeplink
}
