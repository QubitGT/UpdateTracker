using System;
using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform;

public static class RichPresence
{
	public static Request Clear()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_RichPresence_Clear", "");
			return new Request(CAPI.ovr_RichPresence_Clear());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<DestinationList> GetDestinations()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_RichPresence_GetDestinations", "");
			return new Request<DestinationList>(CAPI.ovr_RichPresence_GetDestinations());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request Set(RichPresenceOptions richPresenceOptions)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_RichPresence_Set", "");
			return new Request(CAPI.ovr_RichPresence_Set((IntPtr)richPresenceOptions));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<DestinationList> GetNextDestinationListPage(DestinationList list)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_RichPresence_GetNextDestinationListPage", "");
		if (!list.HasNextPage)
		{
			Debug.LogWarning("Oculus.Platform.GetNextDestinationListPage: List has no next page");
			return null;
		}
		if (Core.IsInitialized())
		{
			return new Request<DestinationList>(CAPI.ovr_HTTP_GetWithMessageType(list.NextUrl, 1731624773));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}
}
