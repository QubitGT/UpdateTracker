using System.ComponentModel;

namespace Oculus.Platform;

public enum UserOrdering
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("NONE")]
	None,
	[Description("PRESENCE_ALPHABETICAL")]
	PresenceAlphabetical
}
