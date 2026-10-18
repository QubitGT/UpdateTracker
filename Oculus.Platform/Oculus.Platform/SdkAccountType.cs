using System.ComponentModel;

namespace Oculus.Platform;

public enum SdkAccountType
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("OCULUS")]
	Oculus,
	[Description("FACEBOOK_GAMEROOM")]
	FacebookGameroom
}
