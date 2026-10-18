using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform;

public static class UserAgeCategory
{
	public static Request<UserAccountAgeCategory> Get()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_UserAgeCategory_Get", "");
			return new Request<UserAccountAgeCategory>(CAPI.ovr_UserAgeCategory_Get());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request Report(AppAgeCategory age_category)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_UserAgeCategory_Report", "");
			return new Request(CAPI.ovr_UserAgeCategory_Report(age_category));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}
}
