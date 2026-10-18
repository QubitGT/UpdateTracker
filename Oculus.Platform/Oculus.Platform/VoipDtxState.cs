using System.ComponentModel;

namespace Oculus.Platform;

public enum VoipDtxState
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("ENABLED")]
	Enabled,
	[Description("DISABLED")]
	Disabled
}
