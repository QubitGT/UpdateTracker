using System;
using System.Runtime.InteropServices;

public class MothershipBeginQuestCallback : QuestBeginLoginV2RequestCompleteDelegateWrapper
{
	public MothershipBeginQuestCallback()
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<PlayerQuestBeginLoginV2Response> callbackPair = gCHandle.Target as CallbackPair<PlayerQuestBeginLoginV2Response>;
			if (wasSuccess)
			{
				PlayerQuestBeginLoginV2Response obj = PlayerQuestBeginLoginV2Response.FromMothershipResponse(response);
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
