using System.ComponentModel;

namespace Oculus.Platform;

public enum NetSyncConnectionStatus
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("CONNECTING")]
	Connecting,
	[Description("DISCONNECTED")]
	Disconnected,
	[Description("CONNECTED")]
	Connected
}
