namespace Fusion.Photon.Realtime;

internal enum DisconnectCause
{
	None,
	ExceptionOnConnect,
	DnsExceptionOnConnect,
	ServerAddressInvalid,
	Exception,
	SendException,
	ReceiveException,
	ServerTimeout,
	ClientTimeout,
	DisconnectByServerLogic,
	DisconnectByServerReasonUnknown,
	InvalidAuthentication,
	CustomAuthenticationFailed,
	AuthenticationTicketExpired,
	MaxCcuReached,
	InvalidRegion,
	OperationNotAllowedInCurrentState,
	DisconnectByClientLogic,
	DisconnectByOperationLimit,
	DisconnectByDisconnectMessage,
	ApplicationQuit
}
