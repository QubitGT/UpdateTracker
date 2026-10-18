using System;
using System.Runtime.InteropServices;

public class MothershipListTitleDataCallback : ListMothershipTitleDataCompleteDelegateWrapper
{
	public MothershipListTitleDataCallback()
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData == IntPtr.Zero)
		{
			return;
		}
		GCHandle gCHandle = (GCHandle)userData;
		try
		{
			CallbackPair<ListClientMothershipTitleDataResponse> callbackPair = gCHandle.Target as CallbackPair<ListClientMothershipTitleDataResponse>;
			if (wasSuccess)
			{
				ListClientMothershipTitleDataResponse obj = ListClientMothershipTitleDataResponse.FromMothershipResponse(response);
				callbackPair.successCallback?.Invoke(obj);
			}
			else
			{
				callbackPair.errorCallback?.Invoke(error, response.statusCode);
			}
		}
		finally
		{
			gCHandle.Free();
		}
	}
}
