using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Steamworks;

public static class GameServer
{
	public static ESteamAPIInitResult InitEx(uint unIP, ushort usGamePort, ushort usQueryPort, EServerMode eServerMode, string pchVersionString, out string OutSteamErrMsg)
	{
		InteropHelp.TestIfPlatformSupported();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("SteamUtils010").Append('\0');
		stringBuilder.Append("SteamNetworkingUtils004").Append('\0');
		stringBuilder.Append("SteamGameServer015").Append('\0');
		stringBuilder.Append("SteamGameServerStats001").Append('\0');
		stringBuilder.Append("STEAMHTTP_INTERFACE_VERSION003").Append('\0');
		stringBuilder.Append("STEAMINVENTORY_INTERFACE_V003").Append('\0');
		stringBuilder.Append("SteamNetworking006").Append('\0');
		stringBuilder.Append("SteamNetworkingMessages002").Append('\0');
		stringBuilder.Append("SteamNetworkingSockets012").Append('\0');
		stringBuilder.Append("STEAMUGC_INTERFACE_VERSION021").Append('\0');
		using InteropHelp.UTF8StringHandle pchVersionString2 = new InteropHelp.UTF8StringHandle(pchVersionString);
		using InteropHelp.UTF8StringHandle pszInternalCheckInterfaceVersions = new InteropHelp.UTF8StringHandle(stringBuilder.ToString());
		IntPtr intPtr = Marshal.AllocHGlobal(1024);
		ESteamAPIInitResult eSteamAPIInitResult = NativeMethods.SteamInternal_GameServer_Init_V2(unIP, usGamePort, usQueryPort, eServerMode, pchVersionString2, pszInternalCheckInterfaceVersions, intPtr);
		OutSteamErrMsg = InteropHelp.PtrToStringUTF8(intPtr);
		Marshal.FreeHGlobal(intPtr);
		if (eSteamAPIInitResult == ESteamAPIInitResult.k_ESteamAPIInitResult_OK)
		{
			if (CSteamGameServerAPIContext.Init())
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

	public static bool Init(uint unIP, ushort usGamePort, ushort usQueryPort, EServerMode eServerMode, string pchVersionString)
	{
		InteropHelp.TestIfPlatformSupported();
		string OutSteamErrMsg;
		return InitEx(unIP, usGamePort, usQueryPort, eServerMode, pchVersionString, out OutSteamErrMsg) == ESteamAPIInitResult.k_ESteamAPIInitResult_OK;
	}

	public static void Shutdown()
	{
		InteropHelp.TestIfPlatformSupported();
		NativeMethods.SteamGameServer_Shutdown();
		CSteamGameServerAPIContext.Clear();
		CallbackDispatcher.Shutdown();
	}

	public static void RunCallbacks()
	{
		CallbackDispatcher.RunFrame(isGameServer: true);
	}

	public static void ReleaseCurrentThreadMemory()
	{
		InteropHelp.TestIfPlatformSupported();
		NativeMethods.SteamGameServer_ReleaseCurrentThreadMemory();
	}

	public static bool BSecure()
	{
		InteropHelp.TestIfPlatformSupported();
		return NativeMethods.SteamGameServer_BSecure();
	}

	public static CSteamID GetSteamID()
	{
		InteropHelp.TestIfPlatformSupported();
		return (CSteamID)NativeMethods.SteamGameServer_GetSteamID();
	}

	public static HSteamPipe GetHSteamPipe()
	{
		InteropHelp.TestIfPlatformSupported();
		return (HSteamPipe)NativeMethods.SteamGameServer_GetHSteamPipe();
	}

	public static HSteamUser GetHSteamUser()
	{
		InteropHelp.TestIfPlatformSupported();
		return (HSteamUser)NativeMethods.SteamGameServer_GetHSteamUser();
	}
}
