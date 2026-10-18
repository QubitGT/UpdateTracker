using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform;

public static class Media
{
	public static Request<ShareMediaResult> ShareToFacebook(string postTextSuggestion, string filePath, MediaContentType contentType)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Media_ShareToFacebook", "");
			return new Request<ShareMediaResult>(CAPI.ovr_Media_ShareToFacebook(postTextSuggestion, filePath, contentType));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}
}
