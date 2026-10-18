using System.ComponentModel;

namespace Oculus.Platform;

public enum ServiceProvider
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("DROPBOX")]
	Dropbox,
	[Description("FACEBOOK")]
	Facebook,
	[Description("GOOGLE")]
	Google,
	[Description("INSTAGRAM")]
	Instagram,
	[Description("REMOTE_MEDIA")]
	RemoteMedia
}
