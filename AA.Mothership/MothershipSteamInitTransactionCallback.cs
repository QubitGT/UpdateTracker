using System;
using System.Runtime.InteropServices;

internal class MothershipSteamInitTransactionCallback : InitSteamPurchaseCompleteDelegateWrapper
{
	public MothershipSteamInitTransactionCallback()
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<InitSteamPurchaseResponse> callbackPair = gCHandle.Target as CallbackPair<InitSteamPurchaseResponse>;
			if (wasSuccess)
			{
				InitSteamPurchaseResponse obj = InitSteamPurchaseResponse.FromMothershipResponse(response);
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
