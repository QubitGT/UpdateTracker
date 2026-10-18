namespace Fusion;

public enum ShutdownReason
{
	Ok,
	Error,
	IncompatibleConfiguration,
	ServerInRoom,
	DisconnectedByPluginLogic,
	GameClosed,
	GameNotFound,
	MaxCcuReached,
	InvalidRegion,
	GameIdAlreadyExists,
	GameIsFull,
	InvalidAuthentication,
	CustomAuthenticationFailed,
	AuthenticationTicketExpired,
	PhotonCloudTimeout,
	AlreadyRunning,
	InvalidArguments,
	HostMigration,
	ConnectionTimeout,
	ConnectionRefused,
	OperationTimeout,
	OperationCanceled
}
