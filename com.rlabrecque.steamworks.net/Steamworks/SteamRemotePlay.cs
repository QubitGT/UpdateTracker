using System;

namespace Steamworks;

public static class SteamRemotePlay
{
	public static uint GetSessionCount()
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamRemotePlay_GetSessionCount(CSteamAPIContext.GetSteamRemotePlay());
	}

	public static RemotePlaySessionID_t GetSessionID(int iSessionIndex)
	{
		InteropHelp.TestIfAvailableClient();
		return (RemotePlaySessionID_t)NativeMethods.ISteamRemotePlay_GetSessionID(CSteamAPIContext.GetSteamRemotePlay(), iSessionIndex);
	}

	public static bool BSessionRemotePlayTogether(RemotePlaySessionID_t unSessionID)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamRemotePlay_BSessionRemotePlayTogether(CSteamAPIContext.GetSteamRemotePlay(), unSessionID);
	}

	public static CSteamID GetSessionSteamID(RemotePlaySessionID_t unSessionID)
	{
		InteropHelp.TestIfAvailableClient();
		return (CSteamID)NativeMethods.ISteamRemotePlay_GetSessionSteamID(CSteamAPIContext.GetSteamRemotePlay(), unSessionID);
	}

	public static uint GetSessionGuestID(RemotePlaySessionID_t unSessionID)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamRemotePlay_GetSessionGuestID(CSteamAPIContext.GetSteamRemotePlay(), unSessionID);
	}

	public static int GetSmallSessionAvatar(RemotePlaySessionID_t unSessionID)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamRemotePlay_GetSmallSessionAvatar(CSteamAPIContext.GetSteamRemotePlay(), unSessionID);
	}

	public static int GetMediumSessionAvatar(RemotePlaySessionID_t unSessionID)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamRemotePlay_GetMediumSessionAvatar(CSteamAPIContext.GetSteamRemotePlay(), unSessionID);
	}

	public static int GetLargeSessionAvatar(RemotePlaySessionID_t unSessionID)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamRemotePlay_GetLargeSessionAvatar(CSteamAPIContext.GetSteamRemotePlay(), unSessionID);
	}

	public static string GetSessionClientName(RemotePlaySessionID_t unSessionID)
	{
		InteropHelp.TestIfAvailableClient();
		return InteropHelp.PtrToStringUTF8(NativeMethods.ISteamRemotePlay_GetSessionClientName(CSteamAPIContext.GetSteamRemotePlay(), unSessionID));
	}

	public static ESteamDeviceFormFactor GetSessionClientFormFactor(RemotePlaySessionID_t unSessionID)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamRemotePlay_GetSessionClientFormFactor(CSteamAPIContext.GetSteamRemotePlay(), unSessionID);
	}

	public static bool BGetSessionClientResolution(RemotePlaySessionID_t unSessionID, out int pnResolutionX, out int pnResolutionY)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamRemotePlay_BGetSessionClientResolution(CSteamAPIContext.GetSteamRemotePlay(), unSessionID, out pnResolutionX, out pnResolutionY);
	}

	public static bool ShowRemotePlayTogetherUI()
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamRemotePlay_ShowRemotePlayTogetherUI(CSteamAPIContext.GetSteamRemotePlay());
	}

	public static bool BSendRemotePlayTogetherInvite(CSteamID steamIDFriend)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamRemotePlay_BSendRemotePlayTogetherInvite(CSteamAPIContext.GetSteamRemotePlay(), steamIDFriend);
	}

	public static bool BEnableRemotePlayTogetherDirectInput()
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamRemotePlay_BEnableRemotePlayTogetherDirectInput(CSteamAPIContext.GetSteamRemotePlay());
	}

	public static void DisableRemotePlayTogetherDirectInput()
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamRemotePlay_DisableRemotePlayTogetherDirectInput(CSteamAPIContext.GetSteamRemotePlay());
	}

	public static uint GetInput(RemotePlayInput_t[] pInput, uint unMaxEvents)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamRemotePlay_GetInput(CSteamAPIContext.GetSteamRemotePlay(), pInput, unMaxEvents);
	}

	public static void SetMouseVisibility(RemotePlaySessionID_t unSessionID, bool bVisible)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamRemotePlay_SetMouseVisibility(CSteamAPIContext.GetSteamRemotePlay(), unSessionID, bVisible);
	}

	public static void SetMousePosition(RemotePlaySessionID_t unSessionID, float flNormalizedX, float flNormalizedY)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamRemotePlay_SetMousePosition(CSteamAPIContext.GetSteamRemotePlay(), unSessionID, flNormalizedX, flNormalizedY);
	}

	public static RemotePlayCursorID_t CreateMouseCursor(int nWidth, int nHeight, int nHotX, int nHotY, IntPtr pBGRA, int nPitch = 0)
	{
		InteropHelp.TestIfAvailableClient();
		return (RemotePlayCursorID_t)NativeMethods.ISteamRemotePlay_CreateMouseCursor(CSteamAPIContext.GetSteamRemotePlay(), nWidth, nHeight, nHotX, nHotY, pBGRA, nPitch);
	}

	public static void SetMouseCursor(RemotePlaySessionID_t unSessionID, RemotePlayCursorID_t unCursorID)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamRemotePlay_SetMouseCursor(CSteamAPIContext.GetSteamRemotePlay(), unSessionID, unCursorID);
	}
}
