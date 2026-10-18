using System;
using System.Runtime.InteropServices;

public class MothershipGetStorefrontCallback : GetStorefrontRequestCompleteDelegateWrapper
{
	public MothershipGetStorefrontCallback()
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<MothershipGetStorefrontResponse> callbackPair = gCHandle.Target as CallbackPair<MothershipGetStorefrontResponse>;
			if (wasSuccess)
			{
				MothershipGetStorefrontResponse obj = MothershipGetStorefrontResponse.FromMothershipResponse(response);
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
