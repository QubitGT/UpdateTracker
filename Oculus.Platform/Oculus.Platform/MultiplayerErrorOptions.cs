using System;

namespace Oculus.Platform;

public class MultiplayerErrorOptions
{
	private IntPtr Handle;

	public MultiplayerErrorOptions()
	{
		Handle = CAPI.ovr_MultiplayerErrorOptions_Create();
	}

	public void SetErrorKey(MultiplayerErrorErrorKey value)
	{
		CAPI.ovr_MultiplayerErrorOptions_SetErrorKey(Handle, value);
	}

	public static explicit operator IntPtr(MultiplayerErrorOptions options)
	{
		return options?.Handle ?? IntPtr.Zero;
	}

	~MultiplayerErrorOptions()
	{
		CAPI.ovr_MultiplayerErrorOptions_Destroy(Handle);
	}
}
