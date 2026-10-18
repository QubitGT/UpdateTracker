using System;

namespace Oculus.Platform;

public class GroupPresenceOptions
{
	private IntPtr Handle;

	public GroupPresenceOptions()
	{
		Handle = CAPI.ovr_GroupPresenceOptions_Create();
	}

	public void SetDeeplinkMessageOverride(string value)
	{
		CAPI.ovr_GroupPresenceOptions_SetDeeplinkMessageOverride(Handle, value);
	}

	public void SetDestinationApiName(string value)
	{
		CAPI.ovr_GroupPresenceOptions_SetDestinationApiName(Handle, value);
	}

	public void SetIsJoinable(bool value)
	{
		CAPI.ovr_GroupPresenceOptions_SetIsJoinable(Handle, value);
	}

	public void SetLobbySessionId(string value)
	{
		CAPI.ovr_GroupPresenceOptions_SetLobbySessionId(Handle, value);
	}

	public void SetMatchSessionId(string value)
	{
		CAPI.ovr_GroupPresenceOptions_SetMatchSessionId(Handle, value);
	}

	public static explicit operator IntPtr(GroupPresenceOptions options)
	{
		return options?.Handle ?? IntPtr.Zero;
	}

	~GroupPresenceOptions()
	{
		CAPI.ovr_GroupPresenceOptions_Destroy(Handle);
	}
}
