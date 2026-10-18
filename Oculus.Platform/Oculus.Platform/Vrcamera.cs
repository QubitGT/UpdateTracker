namespace Oculus.Platform;

public static class Vrcamera
{
	public static void SetGetDataChannelMessageUpdateNotificationCallback(Message<string>.Callback callback)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Vrcamera_GetDataChannelMessageUpdateNotificationCallback", "");
		Callback.SetNotificationCallback(Message.MessageType.Notification_Vrcamera_GetDataChannelMessageUpdate, callback);
	}

	public static void SetGetSurfaceUpdateNotificationCallback(Message<string>.Callback callback)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Vrcamera_GetSurfaceUpdateNotificationCallback", "");
		Callback.SetNotificationCallback(Message.MessageType.Notification_Vrcamera_GetSurfaceUpdate, callback);
	}
}
