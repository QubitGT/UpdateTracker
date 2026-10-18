using UnityEngine;

namespace Oculus.Platform;

public static class Notifications
{
	public static Request MarkAsRead(ulong notificationID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Notifications_MarkAsRead", "");
			return new Request(CAPI.ovr_Notification_MarkAsRead(notificationID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}
}
