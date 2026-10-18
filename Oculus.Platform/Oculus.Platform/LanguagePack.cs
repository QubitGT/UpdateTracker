using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform;

public static class LanguagePack
{
	public static Request<AssetDetails> GetCurrent()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_LanguagePack_GetCurrent", "");
			return new Request<AssetDetails>(CAPI.ovr_LanguagePack_GetCurrent());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AssetFileDownloadResult> SetCurrent(string tag)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_LanguagePack_SetCurrent", "");
			return new Request<AssetFileDownloadResult>(CAPI.ovr_LanguagePack_SetCurrent(tag));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}
}
