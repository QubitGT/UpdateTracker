using System.ComponentModel;

namespace Oculus.Platform;

public enum KeyValuePairType
{
	[Description("STRING")]
	String,
	[Description("INTEGER")]
	Int,
	[Description("DOUBLE")]
	Double,
	[Description("UNKNOWN")]
	Unknown
}
