using System;
using System.Runtime.InteropServices;

public class MothershipBeginSteamCallback : PlayerSteamBeginLoginResponseCompleteDelegateWrapper
{
	public MothershipBeginSteamCallback()
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<PlayerSteamBeginLoginResponse> callbackPair = gCHandle.Target as CallbackPair<PlayerSteamBeginLoginResponse>;
			if (wasSuccess)
			{
				PlayerSteamBeginLoginResponse obj = PlayerSteamBeginLoginResponse.FromMothershipResponse(response);
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
