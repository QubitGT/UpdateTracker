using System.ComponentModel;

namespace Oculus.Platform;

public enum NetSyncVoipStreamMode
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("AMBISONIC")]
	Ambisonic,
	[Description("MONO")]
	Mono
}
