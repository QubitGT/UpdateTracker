using UnityEngine;

namespace Oculus.Platform;

public static class Entitlements
{
	public static Request IsUserEntitledToApplication()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Entitlements_IsUserEntitledToApplication", "");
			return new Request(CAPI.ovr_Entitlement_GetIsViewerEntitled());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}
}
