using System;

namespace Oculus.Platform.Models;

public class NetSyncConnection
{
	public readonly long ConnectionId;

	public readonly NetSyncDisconnectReason DisconnectReason;

	public readonly ulong SessionId;

	public readonly NetSyncConnectionStatus Status;

	public readonly string ZoneId;

	public NetSyncConnection(IntPtr o)
	{
		ConnectionId = CAPI.ovr_NetSyncConnection_GetConnectionId(o);
		DisconnectReason = CAPI.ovr_NetSyncConnection_GetDisconnectReason(o);
		SessionId = CAPI.ovr_NetSyncConnection_GetSessionId(o);
		Status = CAPI.ovr_NetSyncConnection_GetStatus(o);
		ZoneId = CAPI.ovr_NetSyncConnection_GetZoneId(o);
	}
}
