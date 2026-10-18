using System;

namespace Oculus.Platform.Models;

public class AppDownloadResult
{
	public readonly AppInstallResult AppInstallResult;

	public readonly long Timestamp;

	public AppDownloadResult(IntPtr o)
	{
		AppInstallResult = CAPI.ovr_AppDownloadResult_GetAppInstallResult(o);
		Timestamp = CAPI.ovr_AppDownloadResult_GetTimestamp(o);
	}
}
