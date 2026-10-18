using System.ComponentModel;

namespace Oculus.Platform;

public enum SystemVoipStatus
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("UNAVAILABLE")]
	Unavailable,
	[Description("SUPPRESSED")]
	Suppressed,
	[Description("ACTIVE")]
	Active
}
