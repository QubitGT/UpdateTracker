using System.ComponentModel;

namespace Oculus.Platform;

public enum OfferTerm
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("WEEKLY")]
	WEEKLY,
	[Description("BIWEEKLY")]
	BIWEEKLY,
	[Description("MONTHLY")]
	MONTHLY,
	[Description("QUARTERLY")]
	QUARTERLY,
	[Description("SEMIANNUAL")]
	SEMIANNUAL,
	[Description("ANNUAL")]
	ANNUAL,
	[Description("BIANNUAL")]
	BIANNUAL
}
