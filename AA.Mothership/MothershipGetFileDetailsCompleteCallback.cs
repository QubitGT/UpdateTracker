using System;
using System.Runtime.InteropServices;

public class MothershipGetFileDetailsCompleteCallback : GetFileCompleteClientDelegateWrapper
{
	public MothershipGetFileDetailsCompleteCallback()
	{
		swigCMemOwn = false;
	}

	public override void OnCompleteCallback(MothershipResponse response, bool wasSuccess, MothershipError error, IntPtr userData)
	{
		if (userData != IntPtr.Zero)
		{
			GCHandle gCHandle = (GCHandle)userData;
			CallbackPair<SharedDownloadableFileResult> callbackPair = gCHandle.Target as CallbackPair<SharedDownloadableFileResult>;
			if (wasSuccess)
			{
				SharedDownloadableFileResult obj = SharedDownloadableFileResult.FromMothershipResponse(response);
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
