using System.ComponentModel;

namespace Oculus.Platform;

public enum ChallengeCreationType
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("USER_CREATED")]
	UserCreated,
	[Description("DEVELOPER_CREATED")]
	DeveloperCreated
}
