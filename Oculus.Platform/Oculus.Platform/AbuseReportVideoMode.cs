using System.ComponentModel;

namespace Oculus.Platform;

public enum AbuseReportVideoMode
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("COLLECT")]
	Collect,
	[Description("OPTIONAL")]
	Optional,
	[Description("SKIP")]
	Skip
}
