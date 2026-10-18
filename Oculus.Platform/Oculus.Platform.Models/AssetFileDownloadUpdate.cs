using System;

namespace Oculus.Platform.Models;

public class AssetFileDownloadUpdate
{
	public readonly ulong AssetFileId;

	public readonly ulong AssetId;

	public readonly ulong BytesTotal;

	public readonly long BytesTransferred;

	public readonly bool Completed;

	public AssetFileDownloadUpdate(IntPtr o)
	{
		AssetFileId = CAPI.ovr_AssetFileDownloadUpdate_GetAssetFileId(o);
		AssetId = CAPI.ovr_AssetFileDownloadUpdate_GetAssetId(o);
		BytesTotal = CAPI.ovr_AssetFileDownloadUpdate_GetBytesTotalLong(o);
		BytesTransferred = CAPI.ovr_AssetFileDownloadUpdate_GetBytesTransferredLong(o);
		Completed = CAPI.ovr_AssetFileDownloadUpdate_GetCompleted(o);
	}
}
