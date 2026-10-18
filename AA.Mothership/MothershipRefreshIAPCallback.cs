using System;
using System.Runtime.InteropServices;

public class MothershipRefreshIAPCallback : MothershipRefreshIAPCompleteDelegateWrapper
{
	public MothershipRefreshIAPCallback()
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<MothershipRefreshIAPResponse> callbackPair = gCHandle.Target as CallbackPair<MothershipRefreshIAPResponse>;
			if (wasSuccess)
			{
				MothershipRefreshIAPResponse obj = MothershipRefreshIAPResponse.FromMothershipResponse(response);
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
