using System.ComponentModel;

namespace Oculus.Platform;

public enum PartyMicState
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("PARTY")]
	Party,
	[Description("APP")]
	App,
	[Description("MUTE")]
	Mute,
	[Description("INACTIVE")]
	Inactive,
	[Description("INPUT_SHARED")]
	InputShared
}
