using System;
using System.Runtime.InteropServices;

public class MothershipGetMySubscriptionCallback : ClientGetMySubscriptionCompleteDelegateWrapper
{
	public MothershipGetMySubscriptionCallback()
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<GetMySubscriptionsResponse> callbackPair = gCHandle.Target as CallbackPair<GetMySubscriptionsResponse>;
			if (wasSuccess)
			{
				GetMySubscriptionsResponse obj = GetMySubscriptionsResponse.FromMothershipResponse(response);
				callbackPair.successCallback(obj);
			}
			else
			{
				callbackPair.errorCallback(error, response.statusCode);
			}
			gCHandle.Free();
		}
	}
}
