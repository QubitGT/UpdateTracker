using UnityEngine;

namespace Oculus.Platform;

public static class DeviceApplicationIntegrity
{
	public static Request<string> GetIntegrityToken(string challenge_nonce)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_DeviceApplicationIntegrity_GetIntegrityToken", "");
			return new Request<string>(CAPI.ovr_DeviceApplicationIntegrity_GetIntegrityToken(challenge_nonce));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}
}
