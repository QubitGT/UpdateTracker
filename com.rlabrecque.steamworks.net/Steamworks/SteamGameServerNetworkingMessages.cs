using System;

namespace Steamworks;

public static class SteamGameServerNetworkingMessages
{
	public static EResult SendMessageToUser(ref SteamNetworkingIdentity identityRemote, IntPtr pubData, uint cubData, int nSendFlags, int nRemoteChannel)
	{
		InteropHelp.TestIfAvailableGameServer();
		return NativeMethods.ISteamNetworkingMessages_SendMessageToUser(CSteamGameServerAPIContext.GetSteamNetworkingMessages(), ref identityRemote, pubData, cubData, nSendFlags, nRemoteChannel);
	}

	public static int ReceiveMessagesOnChannel(int nLocalChannel, IntPtr[] ppOutMessages, int nMaxMessages)
	{
		InteropHelp.TestIfAvailableGameServer();
		if (ppOutMessages != null && ppOutMessages.Length != nMaxMessages)
		{
			throw new ArgumentException("ppOutMessages must be the same size as nMaxMessages!");
		}
		return NativeMethods.ISteamNetworkingMessages_ReceiveMessagesOnChannel(CSteamGameServerAPIContext.GetSteamNetworkingMessages(), nLocalChannel, ppOutMessages, nMaxMessages);
	}

	public static bool AcceptSessionWithUser(ref SteamNetworkingIdentity identityRemote)
	{
		InteropHelp.TestIfAvailableGameServer();
		return NativeMethods.ISteamNetworkingMessages_AcceptSessionWithUser(CSteamGameServerAPIContext.GetSteamNetworkingMessages(), ref identityRemote);
	}

	public static bool CloseSessionWithUser(ref SteamNetworkingIdentity identityRemote)
	{
		InteropHelp.TestIfAvailableGameServer();
		return NativeMethods.ISteamNetworkingMessages_CloseSessionWithUser(CSteamGameServerAPIContext.GetSteamNetworkingMessages(), ref identityRemote);
	}

	public static bool CloseChannelWithUser(ref SteamNetworkingIdentity identityRemote, int nLocalChannel)
	{
		InteropHelp.TestIfAvailableGameServer();
		return NativeMethods.ISteamNetworkingMessages_CloseChannelWithUser(CSteamGameServerAPIContext.GetSteamNetworkingMessages(), ref identityRemote, nLocalChannel);
	}

	public static ESteamNetworkingConnectionState GetSessionConnectionInfo(ref SteamNetworkingIdentity identityRemote, out SteamNetConnectionInfo_t pConnectionInfo, out SteamNetConnectionRealTimeStatus_t pQuickStatus)
	{
		InteropHelp.TestIfAvailableGameServer();
		return NativeMethods.ISteamNetworkingMessages_GetSessionConnectionInfo(CSteamGameServerAPIContext.GetSteamNetworkingMessages(), ref identityRemote, out pConnectionInfo, out pQuickStatus);
	}
}
