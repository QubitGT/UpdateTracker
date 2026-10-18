using System;

namespace Oculus.Platform.Models;

public class SdkAccount
{
	public readonly SdkAccountType AccountType;

	public readonly ulong UserId;

	public SdkAccount(IntPtr o)
	{
		AccountType = CAPI.ovr_SdkAccount_GetAccountType(o);
		UserId = CAPI.ovr_SdkAccount_GetUserId(o);
	}
}
