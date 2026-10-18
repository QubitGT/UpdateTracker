using System;
using System.Runtime.InteropServices;

public class MothershipGetUserDataCallback : GetUserDataCompleteClientDelegateWrapper
{
	public MothershipGetUserDataCallback(MothershipClientApiClient clientApiClient)
		: base(clientApiClient)
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<MothershipUserData> callbackPair = gCHandle.Target as CallbackPair<MothershipUserData>;
			if (wasSuccess)
			{
				GetUserDataResponse obj = GetUserDataResponse.FromMothershipResponse(response);
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
