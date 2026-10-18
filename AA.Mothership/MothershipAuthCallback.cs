using System;
using System.Runtime.InteropServices;

public class MothershipAuthCallback : LoginCompleteDelegateWrapper
{
	public MothershipAuthCallback(MothershipClientApiClient clientApiClient)
		: base(clientApiClient)
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (!(userData != IntPtr.Zero))
		{
			return;
		}
		GCHandle gCHandle = (GCHandle)userData;
		CallbackPair<LoginResponse> callbackPair = gCHandle.Target as CallbackPair<LoginResponse>;
		if (wasSuccess)
		{
			LoginResponse loginResponse = LoginResponse.FromMothershipResponse(response);
			long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			if (loginResponse.ExpirationTime - num < 890000)
			{
				callbackPair.errorCallback(new MothershipError
				{
					Name = "Authentication Failed",
					Message = "Your system clock has drifted too far into the future. Correct it, then restart the game.",
					MothershipErrorCode = "",
					StatusCode = 400
				}, 400);
			}
			else
			{
				MothershipClientContext.MothershipId = loginResponse.MothershipPlayerId;
				MothershipClientContext.Token = loginResponse.Token;
				callbackPair.successCallback(loginResponse);
			}
		}
		else
		{
			callbackPair.errorCallback(error, response.statusCode);
		}
		gCHandle.Free();
	}
}
