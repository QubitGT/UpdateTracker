using System;

namespace Oculus.Platform.Models;

public class AssetFileDownloadResult
{
	public readonly ulong AssetId;

	public readonly string Filepath;

	public AssetFileDownloadResult(IntPtr o)
	{
		AssetId = CAPI.ovr_AssetFileDownloadResult_GetAssetId(o);
		Filepath = CAPI.ovr_AssetFileDownloadResult_GetFilepath(o);
	}
}
