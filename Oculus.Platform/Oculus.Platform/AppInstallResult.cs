using System.ComponentModel;

namespace Oculus.Platform;

public enum AppInstallResult
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("LOW_STORAGE")]
	LowStorage,
	[Description("NETWORK_ERROR")]
	NetworkError,
	[Description("DUPLICATE_REQUEST")]
	DuplicateRequest,
	[Description("INSTALLER_ERROR")]
	InstallerError,
	[Description("USER_CANCELLED")]
	UserCancelled,
	[Description("AUTHORIZATION_ERROR")]
	AuthorizationError,
	[Description("SUCCESS")]
	Success
}
