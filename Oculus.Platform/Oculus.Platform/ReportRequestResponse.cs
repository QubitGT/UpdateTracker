using System.ComponentModel;

namespace Oculus.Platform;

public enum ReportRequestResponse
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("HANDLED")]
	Handled,
	[Description("UNHANDLED")]
	Unhandled,
	[Description("UNAVAILABLE")]
	Unavailable
}
