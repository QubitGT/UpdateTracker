using System;

namespace Oculus.Platform.Models;

public class LanguagePackInfo
{
	public readonly string EnglishName;

	public readonly string NativeName;

	public readonly string Tag;

	public LanguagePackInfo(IntPtr o)
	{
		EnglishName = CAPI.ovr_LanguagePackInfo_GetEnglishName(o);
		NativeName = CAPI.ovr_LanguagePackInfo_GetNativeName(o);
		Tag = CAPI.ovr_LanguagePackInfo_GetTag(o);
	}
}
