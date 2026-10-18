using System.ComponentModel;

namespace Oculus.Platform;

public enum PartyUpdateAction
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("Join")]
	Join,
	[Description("Leave")]
	Leave,
	[Description("Invite")]
	Invite,
	[Description("Uninvite")]
	Uninvite
}
