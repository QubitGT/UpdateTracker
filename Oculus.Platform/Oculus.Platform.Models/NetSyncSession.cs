using System;

namespace Oculus.Platform.Models;

public class NetSyncSession
{
	public readonly long ConnectionId;

	public readonly bool Muted;

	public readonly ulong SessionId;

	public readonly ulong UserId;

	public readonly string VoipGroup;

	public NetSyncSession(IntPtr o)
	{
		ConnectionId = CAPI.ovr_NetSyncSession_GetConnectionId(o);
		Muted = CAPI.ovr_NetSyncSession_GetMuted(o);
		SessionId = CAPI.ovr_NetSyncSession_GetSessionId(o);
		UserId = CAPI.ovr_NetSyncSession_GetUserId(o);
		VoipGroup = CAPI.ovr_NetSyncSession_GetVoipGroup(o);
	}
}
