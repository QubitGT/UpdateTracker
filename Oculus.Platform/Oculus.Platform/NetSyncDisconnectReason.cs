using System.ComponentModel;

namespace Oculus.Platform;

public enum NetSyncDisconnectReason
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("LOCAL_TERMINATED")]
	LocalTerminated,
	[Description("SERVER_TERMINATED")]
	ServerTerminated,
	[Description("FAILED")]
	Failed,
	[Description("LOST")]
	Lost
}
