namespace Photon.Realtime;

public enum DisconnectCause
{
	None,
	ExceptionOnConnect,
	DnsExceptionOnConnect,
	ServerAddressInvalid,
	Exception,
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
	DisconnectByDisconnectMessage
}
