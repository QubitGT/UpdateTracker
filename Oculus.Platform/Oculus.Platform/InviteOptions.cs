using System;

namespace Oculus.Platform;

public class InviteOptions
{
	private IntPtr Handle;

	public InviteOptions()
	{
		Handle = CAPI.ovr_InviteOptions_Create();
	}

	public void AddSuggestedUser(ulong userID)
	{
		CAPI.ovr_InviteOptions_AddSuggestedUser(Handle, userID);
	}

	public void ClearSuggestedUsers()
	{
		CAPI.ovr_InviteOptions_ClearSuggestedUsers(Handle);
	}

	public static explicit operator IntPtr(InviteOptions options)
	{
		return options?.Handle ?? IntPtr.Zero;
	}

	~InviteOptions()
	{
		CAPI.ovr_InviteOptions_Destroy(Handle);
	}
}
