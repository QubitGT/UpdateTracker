namespace Oculus.Platform;

public static class Livestreaming
{
	public static void SetStatusUpdateNotificationCallback(Message<Oculus.Platform.Models.LivestreamingStatus>.Callback callback)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Livestreaming_StatusUpdateNotificationCallback", "");
		Callback.SetNotificationCallback(Message.MessageType.Notification_Livestreaming_StatusChange, callback);
	}
}
