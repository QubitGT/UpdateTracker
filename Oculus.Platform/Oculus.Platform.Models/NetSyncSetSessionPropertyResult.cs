using System;

namespace Oculus.Platform.Models;

public class NetSyncSetSessionPropertyResult
{
	public readonly NetSyncSession Session;

	public NetSyncSetSessionPropertyResult(IntPtr o)
	{
		Session = new NetSyncSession(CAPI.ovr_NetSyncSetSessionPropertyResult_GetSession(o));
	}
}
