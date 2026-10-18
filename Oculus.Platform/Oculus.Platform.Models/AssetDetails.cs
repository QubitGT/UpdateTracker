using System;

namespace Oculus.Platform.Models;

public class AssetDetails
{
	public readonly ulong AssetId;

	public readonly string AssetType;

	public readonly string DownloadStatus;

	public readonly string Filepath;

	public readonly string IapStatus;

	public readonly LanguagePackInfo LanguageOptional;

	[Obsolete("Deprecated in favor of LanguageOptional")]
	public readonly LanguagePackInfo Language;

	public readonly string Metadata;

	public AssetDetails(IntPtr o)
	{
		AssetId = CAPI.ovr_AssetDetails_GetAssetId(o);
		AssetType = CAPI.ovr_AssetDetails_GetAssetType(o);
		DownloadStatus = CAPI.ovr_AssetDetails_GetDownloadStatus(o);
		Filepath = CAPI.ovr_AssetDetails_GetFilepath(o);
		IapStatus = CAPI.ovr_AssetDetails_GetIapStatus(o);
		IntPtr intPtr = CAPI.ovr_AssetDetails_GetLanguage(o);
		Language = new LanguagePackInfo(intPtr);
		if (intPtr == IntPtr.Zero)
		{
			LanguageOptional = null;
		}
		else
		{
			LanguageOptional = Language;
		}
		Metadata = CAPI.ovr_AssetDetails_GetMetadata(o);
	}
}
