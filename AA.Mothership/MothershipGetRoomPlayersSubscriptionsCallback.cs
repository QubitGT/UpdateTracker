using System;
using System.Runtime.InteropServices;

public class MothershipGetRoomPlayersSubscriptionsCallback : ClientGetBulkSubscriptionsCompleteDelegateWrapper
{
	public MothershipGetRoomPlayersSubscriptionsCallback()
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<BulkGetSubscriptionsResponse> callbackPair = gCHandle.Target as CallbackPair<BulkGetSubscriptionsResponse>;
			if (wasSuccess)
			{
				BulkGetSubscriptionsResponse obj = BulkGetSubscriptionsResponse.FromMothershipResponse(response);
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
