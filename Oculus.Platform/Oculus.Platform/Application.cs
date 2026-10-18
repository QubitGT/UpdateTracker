using System;
using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform;

public static class Application
{
	public static Request<AppDownloadResult> CancelAppDownload()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Application_CancelAppDownload", "");
			return new Request<AppDownloadResult>(CAPI.ovr_Application_CancelAppDownload());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AppDownloadProgressResult> CheckAppDownloadProgress()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Application_CheckAppDownloadProgress", "");
			return new Request<AppDownloadProgressResult>(CAPI.ovr_Application_CheckAppDownloadProgress());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<ApplicationVersion> GetVersion()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Application_GetVersion", "");
			return new Request<ApplicationVersion>(CAPI.ovr_Application_GetVersion());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AppDownloadResult> InstallAppUpdateAndRelaunch(ApplicationOptions deeplink_options = null)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Application_InstallAppUpdateAndRelaunch", "");
			return new Request<AppDownloadResult>(CAPI.ovr_Application_InstallAppUpdateAndRelaunch((IntPtr)deeplink_options));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<string> LaunchOtherApp(ulong appID, ApplicationOptions deeplink_options = null)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Application_LaunchOtherApp", "");
			return new Request<string>(CAPI.ovr_Application_LaunchOtherApp(appID, (IntPtr)deeplink_options));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AppDownloadResult> StartAppDownload()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Application_StartAppDownload", "");
			return new Request<AppDownloadResult>(CAPI.ovr_Application_StartAppDownload());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}
}
