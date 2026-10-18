using System;
using System.Runtime.InteropServices;

public class MothershipConsumeCompleteCallback : ClientConsumeConsumableCompleteDelegateWrapper
{
	public MothershipConsumeCompleteCallback()
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<MothershipConsumeConsumableResponse> callbackPair = gCHandle.Target as CallbackPair<MothershipConsumeConsumableResponse>;
			if (wasSuccess)
			{
				MothershipConsumeConsumableResponse obj = MothershipConsumeConsumableResponse.FromMothershipResponse(response);
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
