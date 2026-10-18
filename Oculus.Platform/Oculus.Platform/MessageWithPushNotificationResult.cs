using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithPushNotificationResult : Message<PushNotificationResult>
{
	public MessageWithPushNotificationResult(IntPtr c_message)
		: base(c_message)
	{
	}

	public override PushNotificationResult GetPushNotificationResult()
	{
		return base.Data;
	}

	protected override PushNotificationResult GetDataFromMessage(IntPtr c_message)
	{
		return new PushNotificationResult(CAPI.ovr_Message_GetPushNotificationResult(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
