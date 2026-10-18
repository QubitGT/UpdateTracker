#define DEBUG
namespace Fusion;

internal static class ErrorCodeExt
{
	public static ShutdownReason ConvertToShutdownReason(short errorCode)
	{
		switch (errorCode)
		{
		case 0:
			return ShutdownReason.Ok;
		case 32751:
			InternalLogStreams.LogDebug?.Error("Fusion plug-in not found. Make sure to use a Fusion-type Photon Application ID.");
			return ShutdownReason.IncompatibleConfiguration;
		case 32766:
			return ShutdownReason.GameIdAlreadyExists;
		case 32758:
		case 32760:
			return ShutdownReason.GameNotFound;
		case 32765:
			return ShutdownReason.GameIsFull;
		case 32764:
			return ShutdownReason.GameClosed;
		case 32757:
			return ShutdownReason.MaxCcuReached;
		case 32756:
			return ShutdownReason.InvalidRegion;
		case short.MaxValue:
			return ShutdownReason.InvalidAuthentication;
		case 32753:
			return ShutdownReason.AuthenticationTicketExpired;
		case 32755:
			return ShutdownReason.CustomAuthenticationFailed;
		case 32752:
			return ShutdownReason.Error;
		case 32762:
			InternalLogStreams.LogDebug?.Error("All servers are busy. This is a temporary issue and the game logic should try again after a brief wait time.");
			return ShutdownReason.Error;
		default:
			return ShutdownReason.Error;
		}
	}
}
