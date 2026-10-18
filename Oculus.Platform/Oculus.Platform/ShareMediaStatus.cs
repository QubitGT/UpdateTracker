using System.ComponentModel;

namespace Oculus.Platform;

public enum ShareMediaStatus
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("SHARED")]
	Shared,
	[Description("CANCELED")]
	Canceled
}
