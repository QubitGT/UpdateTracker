using System;
using System.Runtime.InteropServices;

public class MothershipGetMergedInventoryCallback : GetMergedInventoryCompleteClientDelegateWrapper
{
	public MothershipGetMergedInventoryCallback()
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<MothershipGetMergedInventoryResponse> callbackPair = gCHandle.Target as CallbackPair<MothershipGetMergedInventoryResponse>;
			if (wasSuccess)
			{
				MothershipGetMergedInventoryResponse obj = MothershipGetMergedInventoryResponse.FromMothershipResponse(response);
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
