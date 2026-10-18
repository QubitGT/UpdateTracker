using System;

namespace Oculus.Platform;

public class RosterOptions
{
	private IntPtr Handle;

	public RosterOptions()
	{
		Handle = CAPI.ovr_RosterOptions_Create();
	}

	public void AddSuggestedUser(ulong userID)
	{
		CAPI.ovr_RosterOptions_AddSuggestedUser(Handle, userID);
	}

	public void ClearSuggestedUsers()
	{
		CAPI.ovr_RosterOptions_ClearSuggestedUsers(Handle);
	}

	public static explicit operator IntPtr(RosterOptions options)
	{
		return options?.Handle ?? IntPtr.Zero;
	}

	~RosterOptions()
	{
		CAPI.ovr_RosterOptions_Destroy(Handle);
	}
}
