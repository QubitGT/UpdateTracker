using System;
using System.Runtime.InteropServices;

public class MothershipGetPlayerProgressionCallback : GetProgressionTrackValuesForPlayerCompleteClientDelegateWrapper
{
	public MothershipGetPlayerProgressionCallback()
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<GetProgressionTrackValuesForPlayerResponse> callbackPair = gCHandle.Target as CallbackPair<GetProgressionTrackValuesForPlayerResponse>;
			if (wasSuccess)
			{
				GetProgressionTrackValuesForPlayerResponse obj = GetProgressionTrackValuesForPlayerResponse.FromMothershipResponse(response);
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
