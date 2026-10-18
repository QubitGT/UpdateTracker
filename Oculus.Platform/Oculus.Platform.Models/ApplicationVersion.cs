using System;

namespace Oculus.Platform.Models;

public class ApplicationVersion
{
	public readonly int CurrentCode;

	public readonly string CurrentName;

	public readonly int LatestCode;

	public readonly string LatestName;

	public readonly long ReleaseDate;

	public readonly string Size;

	public ApplicationVersion(IntPtr o)
	{
		CurrentCode = CAPI.ovr_ApplicationVersion_GetCurrentCode(o);
		CurrentName = CAPI.ovr_ApplicationVersion_GetCurrentName(o);
		LatestCode = CAPI.ovr_ApplicationVersion_GetLatestCode(o);
		LatestName = CAPI.ovr_ApplicationVersion_GetLatestName(o);
		ReleaseDate = CAPI.ovr_ApplicationVersion_GetReleaseDate(o);
		Size = CAPI.ovr_ApplicationVersion_GetSize(o);
	}
}
