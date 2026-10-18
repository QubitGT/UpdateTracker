using Fusion.Protocol;

namespace Fusion;

internal static class DisconnectReasonExt
{
	public static ShutdownReason ConvertToShutdownReason(DisconnectReason disconnectCause)
	{
		return disconnectCause switch
		{
			DisconnectReason.IncompatibleConfiguration => ShutdownReason.IncompatibleConfiguration, 
			DisconnectReason.ServerAlreadyInRoom => ShutdownReason.ServerInRoom, 
			DisconnectReason.ServerLogic => ShutdownReason.DisconnectedByPluginLogic, 
			_ => ShutdownReason.Error, 
		};
	}
}
