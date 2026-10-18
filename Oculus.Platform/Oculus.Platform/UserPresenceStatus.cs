using System.ComponentModel;

namespace Oculus.Platform;

public enum UserPresenceStatus
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("ONLINE")]
	Online,
	[Description("OFFLINE")]
	Offline
}
