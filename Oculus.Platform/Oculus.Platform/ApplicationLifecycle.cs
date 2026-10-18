using Oculus.Platform.Models;

namespace Oculus.Platform;

public static class ApplicationLifecycle
{
	public static LaunchDetails GetLaunchDetails()
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_ApplicationLifecycle_GetLaunchDetails", "");
		return new LaunchDetails(CAPI.ovr_ApplicationLifecycle_GetLaunchDetails());
	}

	public static void LogDeeplinkResult(string trackingID, LaunchResult result)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_ApplicationLifecycle_LogDeeplinkResult", "");
		CAPI.ovr_ApplicationLifecycle_LogDeeplinkResult(trackingID, result);
	}

	public static void SetLaunchIntentChangedNotificationCallback(Message<string>.Callback callback)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_ApplicationLifecycle_LaunchIntentChangedNotificationCallback", "");
		Callback.SetNotificationCallback(Message.MessageType.Notification_ApplicationLifecycle_LaunchIntentChanged, callback);
	}
}
