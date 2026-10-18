using System.ComponentModel;

namespace Oculus.Platform;

public enum PermissionGrantStatus
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("GRANTED")]
	Granted,
	[Description("DENIED")]
	Denied,
	[Description("BLOCKED")]
	Blocked
}
