using System;

namespace Oculus.Platform.Models;

public class UserAccountAgeCategory
{
	public readonly AccountAgeCategory AgeCategory;

	public UserAccountAgeCategory(IntPtr o)
	{
		AgeCategory = CAPI.ovr_UserAccountAgeCategory_GetAgeCategory(o);
	}
}
