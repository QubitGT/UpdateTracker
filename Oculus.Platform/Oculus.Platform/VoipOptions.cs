using System;

namespace Oculus.Platform;

public class VoipOptions
{
	private IntPtr Handle;

	public VoipOptions()
	{
		Handle = CAPI.ovr_VoipOptions_Create();
	}

	public void SetBitrateForNewConnections(VoipBitrate value)
	{
		CAPI.ovr_VoipOptions_SetBitrateForNewConnections(Handle, value);
	}

	public void SetCreateNewConnectionUseDtx(VoipDtxState value)
	{
		CAPI.ovr_VoipOptions_SetCreateNewConnectionUseDtx(Handle, value);
	}

	public static explicit operator IntPtr(VoipOptions options)
	{
		return options?.Handle ?? IntPtr.Zero;
	}

	~VoipOptions()
	{
		CAPI.ovr_VoipOptions_Destroy(Handle);
	}
}
