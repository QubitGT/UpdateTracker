using System;
using System.Runtime.InteropServices;

public class MothershipPurchaseOfferCallback : PurchaseOfferRequestCompleteDelegateWrapper
{
	public MothershipPurchaseOfferCallback()
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<MothershipPurchaseOfferResponse> callbackPair = gCHandle.Target as CallbackPair<MothershipPurchaseOfferResponse>;
			if (wasSuccess)
			{
				MothershipPurchaseOfferResponse obj = MothershipPurchaseOfferResponse.FromMothershipResponse(response);
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
