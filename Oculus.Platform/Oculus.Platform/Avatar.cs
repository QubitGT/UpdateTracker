using System;
using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform;

public static class Avatar
{
	public static Request<AvatarEditorResult> LaunchAvatarEditor(AvatarEditorOptions options = null)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Avatar_LaunchAvatarEditor", "");
			return new Request<AvatarEditorResult>(CAPI.ovr_Avatar_LaunchAvatarEditor((IntPtr)options));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}
}
