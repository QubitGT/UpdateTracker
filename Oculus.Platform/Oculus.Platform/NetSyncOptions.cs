using System;

namespace Oculus.Platform;

public class NetSyncOptions
{
	private IntPtr Handle;

	public NetSyncOptions()
	{
		Handle = CAPI.ovr_NetSyncOptions_Create();
	}

	public void SetVoipGroup(string value)
	{
		CAPI.ovr_NetSyncOptions_SetVoipGroup(Handle, value);
	}

	public void SetVoipStreamDefault(NetSyncVoipStreamMode value)
	{
		CAPI.ovr_NetSyncOptions_SetVoipStreamDefault(Handle, value);
	}

	public void SetZoneId(string value)
	{
		CAPI.ovr_NetSyncOptions_SetZoneId(Handle, value);
	}

	public static explicit operator IntPtr(NetSyncOptions options)
	{
		return options?.Handle ?? IntPtr.Zero;
	}

	~NetSyncOptions()
	{
		CAPI.ovr_NetSyncOptions_Destroy(Handle);
	}
}
