using Fusion.Photon.Realtime;

namespace Fusion;

internal static class DisconnectCauseExt
{
	public static ShutdownReason ConvertToShutdownReason(DisconnectCause disconnectCause)
	{
		switch (disconnectCause)
		{
		case DisconnectCause.None:
		case DisconnectCause.DisconnectByClientLogic:
			return ShutdownReason.Ok;
		case DisconnectCause.ExceptionOnConnect:
		case DisconnectCause.DnsExceptionOnConnect:
		case DisconnectCause.ServerAddressInvalid:
		case DisconnectCause.Exception:
		case DisconnectCause.OperationNotAllowedInCurrentState:
		case DisconnectCause.DisconnectByOperationLimit:
			return ShutdownReason.Error;
		case DisconnectCause.ServerTimeout:
		case DisconnectCause.ClientTimeout:
			return ShutdownReason.PhotonCloudTimeout;
		case DisconnectCause.DisconnectByServerLogic:
		case DisconnectCause.DisconnectByServerReasonUnknown:
		case DisconnectCause.DisconnectByDisconnectMessage:
			return ShutdownReason.DisconnectedByPluginLogic;
		case DisconnectCause.InvalidAuthentication:
			return ShutdownReason.InvalidAuthentication;
		case DisconnectCause.CustomAuthenticationFailed:
			return ShutdownReason.CustomAuthenticationFailed;
		case DisconnectCause.AuthenticationTicketExpired:
			return ShutdownReason.AuthenticationTicketExpired;
		case DisconnectCause.MaxCcuReached:
			return ShutdownReason.MaxCcuReached;
		case DisconnectCause.InvalidRegion:
			return ShutdownReason.InvalidRegion;
		default:
			return ShutdownReason.Ok;
		}
	}
}
