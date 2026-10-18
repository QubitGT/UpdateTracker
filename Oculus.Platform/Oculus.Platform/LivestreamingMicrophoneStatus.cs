using System.ComponentModel;

namespace Oculus.Platform;

public enum LivestreamingMicrophoneStatus
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("MICROPHONE_ON")]
	MicrophoneOn,
	[Description("MICROPHONE_OFF")]
	MicrophoneOff
}
