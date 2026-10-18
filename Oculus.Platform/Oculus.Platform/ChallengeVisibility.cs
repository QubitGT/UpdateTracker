using System.ComponentModel;

namespace Oculus.Platform;

public enum ChallengeVisibility
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("INVITE_ONLY")]
	InviteOnly,
	[Description("PUBLIC")]
	Public,
	[Description("PRIVATE")]
	Private
}
