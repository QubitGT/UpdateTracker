using System.ComponentModel;

namespace Oculus.Platform;

public enum AppStatus
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("ENTITLED")]
	Entitled,
	[Description("DOWNLOAD_QUEUED")]
	DownloadQueued,
	[Description("DOWNLOADING")]
	Downloading,
	[Description("INSTALLING")]
	Installing,
	[Description("INSTALLED")]
	Installed,
	[Description("UNINSTALLING")]
	Uninstalling,
	[Description("INSTALL_QUEUED")]
	InstallQueued
}
