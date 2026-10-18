using System;
using System.Runtime.InteropServices;

public class MothershipGetPlayerProgressionTressCallback : GetProgressionTreesForPlayerCompleteClientDelegateWrapper
{
	public MothershipGetPlayerProgressionTressCallback()
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<GetProgressionTreesForPlayerResponse> callbackPair = gCHandle.Target as CallbackPair<GetProgressionTreesForPlayerResponse>;
			if (wasSuccess)
			{
				GetProgressionTreesForPlayerResponse obj = GetProgressionTreesForPlayerResponse.FromMothershipResponse(response);
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
