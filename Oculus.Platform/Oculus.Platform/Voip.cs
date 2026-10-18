using System;
using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform;

public static class Voip
{
	public static void Start(ulong userID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Voip_Start", "");
			CAPI.ovr_Voip_Start(userID);
		}
	}

	public static void Accept(ulong userID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Voip_Accept", "");
			CAPI.ovr_Voip_Accept(userID);
		}
	}

	public static void Stop(ulong userID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Voip_Stop", "");
			CAPI.ovr_Voip_Stop(userID);
		}
	}

	public static void SetMicrophoneFilterCallback(CAPI.FilterCallback callback)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Voip_SetMicrophoneFilterCallback", "");
			CAPI.ovr_Voip_SetMicrophoneFilterCallbackWithFixedSizeBuffer(callback, (UIntPtr)480uL);
		}
	}

	public static void SetMicrophoneMuted(VoipMuteState state)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Voip_SetMicrophoneMuted", "");
			CAPI.ovr_Voip_SetMicrophoneMuted(state);
		}
	}

	public static VoipMuteState GetSystemVoipMicrophoneMuted()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Voip_GetSystemVoipMicrophoneMuted", "");
			return CAPI.ovr_Voip_GetSystemVoipMicrophoneMuted();
		}
		return VoipMuteState.Unknown;
	}

	public static SystemVoipStatus GetSystemVoipStatus()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Voip_GetSystemVoipStatus", "");
			return CAPI.ovr_Voip_GetSystemVoipStatus();
		}
		return SystemVoipStatus.Unknown;
	}

	public static VoipDtxState GetIsConnectionUsingDtx(ulong peerID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Voip_GetIsConnectionUsingDtx", "");
			return CAPI.ovr_Voip_GetIsConnectionUsingDtx(peerID);
		}
		return VoipDtxState.Unknown;
	}

	public static VoipBitrate GetLocalBitrate(ulong peerID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Voip_GetLocalBitrate", "");
			return CAPI.ovr_Voip_GetLocalBitrate(peerID);
		}
		return VoipBitrate.Unknown;
	}

	public static VoipBitrate GetRemoteBitrate(ulong peerID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Voip_GetRemoteBitrate", "");
			return CAPI.ovr_Voip_GetRemoteBitrate(peerID);
		}
		return VoipBitrate.Unknown;
	}

	public static void SetNewConnectionOptions(VoipOptions voipOptions)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Voip_SetNewConnectionOptions", "");
			CAPI.ovr_Voip_SetNewConnectionOptions((IntPtr)voipOptions);
		}
	}

	public static Request<MicrophoneAvailabilityState> GetMicrophoneAvailability()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Voip_GetMicrophoneAvailability", "");
			return new Request<MicrophoneAvailabilityState>(CAPI.ovr_Voip_GetMicrophoneAvailability());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<SystemVoipState> SetSystemVoipSuppressed(bool suppressed)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Voip_SetSystemVoipSuppressed", "");
			return new Request<SystemVoipState>(CAPI.ovr_Voip_SetSystemVoipSuppressed(suppressed));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static void SetMicrophoneAvailabilityStateUpdateNotificationCallback(Message<string>.Callback callback)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Voip_MicrophoneAvailabilityStateUpdateNotificationCallback", "");
		Callback.SetNotificationCallback(Message.MessageType.Notification_Voip_MicrophoneAvailabilityStateUpdate, callback);
	}

	public static void SetSystemVoipStateNotificationCallback(Message<SystemVoipState>.Callback callback)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Voip_SystemVoipStateNotificationCallback", "");
		Callback.SetNotificationCallback(Message.MessageType.Notification_Voip_SystemVoipState, callback);
	}
}
