using System;

namespace Oculus.Platform;

public class RichPresenceOptions
{
	private IntPtr Handle;

	public RichPresenceOptions()
	{
		Handle = CAPI.ovr_RichPresenceOptions_Create();
	}

	public static explicit operator IntPtr(RichPresenceOptions options)
	{
		return options?.Handle ?? IntPtr.Zero;
	}

	~RichPresenceOptions()
	{
		CAPI.ovr_RichPresenceOptions_Destroy(Handle);
	}
}
