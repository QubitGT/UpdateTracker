using UnityEngine;

namespace Oculus.Platform;

public static class AbuseReport
{
	public static Request ReportRequestHandled(ReportRequestResponse response)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_AbuseReport_ReportRequestHandled", "");
			return new Request(CAPI.ovr_AbuseReport_ReportRequestHandled(response));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static void SetReportButtonPressedNotificationCallback(Message<string>.Callback callback)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_AbuseReport_ReportButtonPressedNotificationCallback", "");
		Callback.SetNotificationCallback(Message.MessageType.Notification_AbuseReport_ReportButtonPressed, callback);
	}
}
