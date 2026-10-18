using System.ComponentModel;

namespace Oculus.Platform;

public enum LivestreamingAudience
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("PUBLIC")]
	Public,
	[Description("FRIENDS")]
	Friends,
	[Description("ONLY_ME")]
	OnlyMe
}
