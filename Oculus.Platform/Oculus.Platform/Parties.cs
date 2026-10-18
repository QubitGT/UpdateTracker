namespace Oculus.Platform;

public static class Parties
{
	public static void SetPartyUpdateNotificationCallback(Message<Oculus.Platform.Models.PartyUpdateNotification>.Callback callback)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Parties_PartyUpdateNotificationCallback", "");
		Callback.SetNotificationCallback(Message.MessageType.Notification_Party_PartyUpdate, callback);
	}
}
