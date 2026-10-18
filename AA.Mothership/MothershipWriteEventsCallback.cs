using System;
using System.Runtime.InteropServices;

public class MothershipWriteEventsCallback : WriteEventsCompleteClientDelegateWrapper
{
	public MothershipWriteEventsCallback()
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<MothershipWriteEventsResponse> callbackPair = gCHandle.Target as CallbackPair<MothershipWriteEventsResponse>;
			if (wasSuccess)
			{
				MothershipWriteEventsResponse obj = MothershipWriteEventsResponse.FromMothershipResponse(response);
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
