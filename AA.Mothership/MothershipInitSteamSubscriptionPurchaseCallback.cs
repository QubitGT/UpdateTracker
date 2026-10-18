using System;
using System.Runtime.InteropServices;

public class MothershipInitSteamSubscriptionPurchaseCallback : ClientInitSteamSubscriptionPurchaseCompleteDelegateWrapper
{
	public MothershipInitSteamSubscriptionPurchaseCallback()
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<InitSteamSubscriptionPurchaseResponse> callbackPair = gCHandle.Target as CallbackPair<InitSteamSubscriptionPurchaseResponse>;
			if (wasSuccess)
			{
				InitSteamSubscriptionPurchaseResponse obj = InitSteamSubscriptionPurchaseResponse.FromMothershipResponse(response);
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
