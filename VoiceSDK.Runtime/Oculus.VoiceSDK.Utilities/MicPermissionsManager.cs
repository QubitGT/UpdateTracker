using System;

namespace Oculus.VoiceSDK.Utilities;

public static class MicPermissionsManager
{
	public static bool HasMicPermission()
	{
		return true;
	}

	public static void RequestMicPermission(Action<string> permissionGrantedCallback = null)
	{
		permissionGrantedCallback?.Invoke("android.permission.RECORD_AUDIO");
	}
}
