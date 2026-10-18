using System;
using System.Runtime.InteropServices;

public class MothershipSetUserDataCallback : SetUserDataCompleteClientDelegateWrapper
{
	public MothershipSetUserDataCallback(MothershipClientApiClient clientApiClient)
		: base(clientApiClient)
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<SetUserDataResponse> callbackPair = gCHandle.Target as CallbackPair<SetUserDataResponse>;
			if (wasSuccess)
			{
				SetUserDataResponse obj = SetUserDataResponse.FromMothershipResponse(response);
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
