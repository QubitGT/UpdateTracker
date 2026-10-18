using System;

namespace Oculus.Platform.Models;

public class LinkedAccount
{
	public readonly string AccessToken;

	public readonly ServiceProvider ServiceProvider;

	public readonly string UserId;

	public LinkedAccount(IntPtr o)
	{
		AccessToken = CAPI.ovr_LinkedAccount_GetAccessToken(o);
		ServiceProvider = CAPI.ovr_LinkedAccount_GetServiceProvider(o);
		UserId = CAPI.ovr_LinkedAccount_GetUserId(o);
	}
}
