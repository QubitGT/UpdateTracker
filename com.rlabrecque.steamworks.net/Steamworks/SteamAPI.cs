using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Steamworks;

public static class SteamAPI
{
	public static ESteamAPIInitResult InitEx(out string OutSteamErrMsg)
	{
		InteropHelp.TestIfPlatformSupported();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("SteamUtils010").Append("\0");
		stringBuilder.Append("SteamNetworkingUtils004").Append("\0");
		stringBuilder.Append("STEAMAPPS_INTERFACE_VERSION009").Append("\0");
		stringBuilder.Append("SteamFriends018").Append("\0");
		stringBuilder.Append("STEAMHTMLSURFACE_INTERFACE_VERSION_005").Append("\0");
		stringBuilder.Append("STEAMHTTP_INTERFACE_VERSION003").Append("\0");
		stringBuilder.Append("SteamInput006").Append("\0");
		stringBuilder.Append("STEAMINVENTORY_INTERFACE_V003").Append("\0");
		stringBuilder.Append("SteamMatchMakingServers002").Append("\0");
		stringBuilder.Append("SteamMatchMaking009").Append("\0");
		stringBuilder.Append("STEAMMUSIC_INTERFACE_VERSION001").Append("\0");
		stringBuilder.Append("SteamNetworkingMessages002").Append("\0");
		stringBuilder.Append("SteamNetworkingSockets012").Append("\0");
		stringBuilder.Append("SteamNetworking006").Append("\0");
		stringBuilder.Append("STEAMPARENTALSETTINGS_INTERFACE_VERSION001").Append("\0");
		stringBuilder.Append("SteamParties002").Append("\0");
		stringBuilder.Append("STEAMREMOTEPLAY_INTERFACE_VERSION004").Append("\0");
		stringBuilder.Append("STEAMREMOTESTORAGE_INTERFACE_VERSION016").Append("\0");
		stringBuilder.Append("STEAMSCREENSHOTS_INTERFACE_VERSION003").Append("\0");
		stringBuilder.Append("STEAMUGC_INTERFACE_VERSION021").Append("\0");
		stringBuilder.Append("STEAMUSERSTATS_INTERFACE_VERSION013").Append("\0");
		stringBuilder.Append("SteamUser023").Append("\0");
		stringBuilder.Append("STEAMVIDEO_INTERFACE_V007").Append("\0");
		using InteropHelp.UTF8StringHandle pszInternalCheckInterfaceVersions = new InteropHelp.UTF8StringHandle(stringBuilder.ToString());
		IntPtr intPtr = Marshal.AllocHGlobal(1024);
		ESteamAPIInitResult eSteamAPIInitResult = NativeMethods.SteamInternal_SteamAPI_Init(pszInternalCheckInterfaceVersions, intPtr);
		OutSteamErrMsg = InteropHelp.PtrToStringUTF8(intPtr);
		Marshal.FreeHGlobal(intPtr);
		if (eSteamAPIInitResult == ESteamAPIInitResult.k_ESteamAPIInitResult_OK)
		{
			if (CSteamAPIContext.Init())
			{
				CallbackDispatcher.Initialize();
			}
			else
			{
				eSteamAPIInitResult = ESteamAPIInitResult.k_ESteamAPIInitResult_FailedGeneric;
				OutSteamErrMsg = "[Steamworks.NET] Failed to initialize CSteamAPIContext";
			}
		}
		return eSteamAPIInitResult;
	}

	public static bool Init()
	{
		InteropHelp.TestIfPlatformSupported();
		string OutSteamErrMsg;
		return InitEx(out OutSteamErrMsg) == ESteamAPIInitResult.k_ESteamAPIInitResult_OK;
	}

	public static void Shutdown()
	{
		InteropHelp.TestIfPlatformSupported();
		NativeMethods.SteamAPI_Shutdown();
		CSteamAPIContext.Clear();
		CallbackDispatcher.Shutdown();
	}

	public static bool RestartAppIfNecessary(AppId_t unOwnAppID)
	{
		InteropHelp.TestIfPlatformSupported();
		return NativeMethods.SteamAPI_RestartAppIfNecessary(unOwnAppID);
	}

	public static void ReleaseCurrentThreadMemory()
	{
		InteropHelp.TestIfPlatformSupported();
		NativeMethods.SteamAPI_ReleaseCurrentThreadMemory();
	}

	public static void RunCallbacks()
	{
		CallbackDispatcher.RunFrame(isGameServer: false);
	}

	public static bool IsSteamRunning()
	{
		InteropHelp.TestIfPlatformSupported();
		return NativeMethods.SteamAPI_IsSteamRunning();
	}

	public static HSteamPipe GetHSteamPipe()
	{
		InteropHelp.TestIfPlatformSupported();
		return (HSteamPipe)NativeMethods.SteamAPI_GetHSteamPipe();
	}

	public static HSteamUser GetHSteamUser()
	{
		InteropHelp.TestIfPlatformSupported();
		return (HSteamUser)NativeMethods.SteamAPI_GetHSteamUser();
	}
}
