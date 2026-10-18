using System;

namespace Oculus.Platform.Models;

public class AppDownloadProgressResult
{
	public readonly long DownloadBytes;

	public readonly long DownloadedBytes;

	public readonly AppStatus StatusCode;

	public AppDownloadProgressResult(IntPtr o)
	{
		DownloadBytes = CAPI.ovr_AppDownloadProgressResult_GetDownloadBytes(o);
		DownloadedBytes = CAPI.ovr_AppDownloadProgressResult_GetDownloadedBytes(o);
		StatusCode = CAPI.ovr_AppDownloadProgressResult_GetStatusCode(o);
	}
}
