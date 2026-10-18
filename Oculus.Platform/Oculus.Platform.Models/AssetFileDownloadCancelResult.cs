using System;

namespace Oculus.Platform.Models;

public class AssetFileDownloadCancelResult
{
	public readonly ulong AssetFileId;

	public readonly ulong AssetId;

	public readonly string Filepath;

	public readonly bool Success;

	public AssetFileDownloadCancelResult(IntPtr o)
	{
		AssetFileId = CAPI.ovr_AssetFileDownloadCancelResult_GetAssetFileId(o);
		AssetId = CAPI.ovr_AssetFileDownloadCancelResult_GetAssetId(o);
		Filepath = CAPI.ovr_AssetFileDownloadCancelResult_GetFilepath(o);
		Success = CAPI.ovr_AssetFileDownloadCancelResult_GetSuccess(o);
	}
}
