using System.ComponentModel;

namespace Oculus.Platform;

public enum ProductType
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("DURABLE")]
	DURABLE,
	[Description("CONSUMABLE")]
	CONSUMABLE,
	[Description("SUBSCRIPTION")]
	SUBSCRIPTION
}
