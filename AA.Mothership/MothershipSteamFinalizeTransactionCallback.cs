using System;
using System.Runtime.InteropServices;

internal class MothershipSteamFinalizeTransactionCallback : FinalizeSteamPurchaseCompleteDelegateWrapper
{
	public MothershipSteamFinalizeTransactionCallback()
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<FinalizeSteamPurchaseResponse> callbackPair = gCHandle.Target as CallbackPair<FinalizeSteamPurchaseResponse>;
			if (wasSuccess)
			{
				FinalizeSteamPurchaseResponse obj = FinalizeSteamPurchaseResponse.FromMothershipResponse(response);
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
