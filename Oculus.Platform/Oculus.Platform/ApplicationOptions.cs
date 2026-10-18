using System;

namespace Oculus.Platform;

public class ApplicationOptions
{
	private IntPtr Handle;

	public ApplicationOptions()
	{
		Handle = CAPI.ovr_ApplicationOptions_Create();
	}

	public void SetDeeplinkMessage(string value)
	{
		CAPI.ovr_ApplicationOptions_SetDeeplinkMessage(Handle, value);
	}

	public void SetDestinationApiName(string value)
	{
		CAPI.ovr_ApplicationOptions_SetDestinationApiName(Handle, value);
	}

	public void SetLobbySessionId(string value)
	{
		CAPI.ovr_ApplicationOptions_SetLobbySessionId(Handle, value);
	}

	public void SetMatchSessionId(string value)
	{
		CAPI.ovr_ApplicationOptions_SetMatchSessionId(Handle, value);
	}

	public void SetRoomId(ulong value)
	{
		CAPI.ovr_ApplicationOptions_SetRoomId(Handle, value);
	}

	public static explicit operator IntPtr(ApplicationOptions options)
	{
		return options?.Handle ?? IntPtr.Zero;
	}

	~ApplicationOptions()
	{
		CAPI.ovr_ApplicationOptions_Destroy(Handle);
	}
}
