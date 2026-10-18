using System;

namespace Oculus.Platform.Models;

public class PushNotificationResult
{
	public readonly string Id;

	public PushNotificationResult(IntPtr o)
	{
		Id = CAPI.ovr_PushNotificationResult_GetId(o);
	}
}
