using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform;

public static class AssetFile
{
	public static Request<AssetFileDeleteResult> Delete(ulong assetFileID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_AssetFile_Delete", "");
			return new Request<AssetFileDeleteResult>(CAPI.ovr_AssetFile_Delete(assetFileID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AssetFileDeleteResult> DeleteById(ulong assetFileID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_AssetFile_DeleteById", "");
			return new Request<AssetFileDeleteResult>(CAPI.ovr_AssetFile_DeleteById(assetFileID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AssetFileDeleteResult> DeleteByName(string assetFileName)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_AssetFile_DeleteByName", "");
			return new Request<AssetFileDeleteResult>(CAPI.ovr_AssetFile_DeleteByName(assetFileName));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AssetFileDownloadResult> Download(ulong assetFileID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_AssetFile_Download", "");
			return new Request<AssetFileDownloadResult>(CAPI.ovr_AssetFile_Download(assetFileID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AssetFileDownloadResult> DownloadById(ulong assetFileID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_AssetFile_DownloadById", "");
			return new Request<AssetFileDownloadResult>(CAPI.ovr_AssetFile_DownloadById(assetFileID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AssetFileDownloadResult> DownloadByName(string assetFileName)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_AssetFile_DownloadByName", "");
			return new Request<AssetFileDownloadResult>(CAPI.ovr_AssetFile_DownloadByName(assetFileName));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AssetFileDownloadCancelResult> DownloadCancel(ulong assetFileID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_AssetFile_DownloadCancel", "");
			return new Request<AssetFileDownloadCancelResult>(CAPI.ovr_AssetFile_DownloadCancel(assetFileID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AssetFileDownloadCancelResult> DownloadCancelById(ulong assetFileID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_AssetFile_DownloadCancelById", "");
			return new Request<AssetFileDownloadCancelResult>(CAPI.ovr_AssetFile_DownloadCancelById(assetFileID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AssetFileDownloadCancelResult> DownloadCancelByName(string assetFileName)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_AssetFile_DownloadCancelByName", "");
			return new Request<AssetFileDownloadCancelResult>(CAPI.ovr_AssetFile_DownloadCancelByName(assetFileName));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AssetDetailsList> GetList()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_AssetFile_GetList", "");
			return new Request<AssetDetailsList>(CAPI.ovr_AssetFile_GetList());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AssetDetails> Status(ulong assetFileID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_AssetFile_Status", "");
			return new Request<AssetDetails>(CAPI.ovr_AssetFile_Status(assetFileID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AssetDetails> StatusById(ulong assetFileID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_AssetFile_StatusById", "");
			return new Request<AssetDetails>(CAPI.ovr_AssetFile_StatusById(assetFileID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AssetDetails> StatusByName(string assetFileName)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_AssetFile_StatusByName", "");
			return new Request<AssetDetails>(CAPI.ovr_AssetFile_StatusByName(assetFileName));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static void SetDownloadUpdateNotificationCallback(Message<AssetFileDownloadUpdate>.Callback callback)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_AssetFile_DownloadUpdateNotificationCallback", "");
		Callback.SetNotificationCallback(Message.MessageType.Notification_AssetFile_DownloadUpdate, callback);
	}
}
