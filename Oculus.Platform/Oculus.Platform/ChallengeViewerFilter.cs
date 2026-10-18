using System.ComponentModel;

namespace Oculus.Platform;

public enum ChallengeViewerFilter
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("ALL_VISIBLE")]
	AllVisible,
	[Description("PARTICIPATING")]
	Participating,
	[Description("INVITED")]
	Invited,
	[Description("PARTICIPATING_OR_INVITED")]
	ParticipatingOrInvited
}
