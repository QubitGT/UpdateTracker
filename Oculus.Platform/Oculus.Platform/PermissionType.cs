using System.ComponentModel;

namespace Oculus.Platform;

public enum PermissionType
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("MICROPHONE")]
	Microphone,
	[Description("WRITE_EXTERNAL_STORAGE")]
	WriteExternalStorage
}
