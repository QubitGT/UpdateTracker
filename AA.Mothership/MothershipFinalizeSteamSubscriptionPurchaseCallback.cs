using System;
using System.Runtime.InteropServices;

public class MothershipFinalizeSteamSubscriptionPurchaseCallback : ClientFinalizeSteamSubscriptionPurchaseCompleteDelegateWrapper
{
	public MothershipFinalizeSteamSubscriptionPurchaseCallback()
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<FinalizeSteamSubscriptionPurchaseResponse> callbackPair = gCHandle.Target as CallbackPair<FinalizeSteamSubscriptionPurchaseResponse>;
			if (wasSuccess)
			{
				FinalizeSteamSubscriptionPurchaseResponse obj = FinalizeSteamSubscriptionPurchaseResponse.FromMothershipResponse(response);
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
