using System.ComponentModel;

namespace Oculus.Platform;

public enum OfferType
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("INTRO_OFFER")]
	INTROOFFER,
	[Description("FREE_TRIAL")]
	FREETRIAL
}
