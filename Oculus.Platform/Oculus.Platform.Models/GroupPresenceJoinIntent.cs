using System;

namespace Oculus.Platform.Models;

public class GroupPresenceJoinIntent
{
	public readonly string DeeplinkMessage;

	public readonly string DestinationApiName;

	public readonly string LobbySessionId;

	public readonly string MatchSessionId;

	public GroupPresenceJoinIntent(IntPtr o)
	{
		DeeplinkMessage = CAPI.ovr_GroupPresenceJoinIntent_GetDeeplinkMessage(o);
		DestinationApiName = CAPI.ovr_GroupPresenceJoinIntent_GetDestinationApiName(o);
		LobbySessionId = CAPI.ovr_GroupPresenceJoinIntent_GetLobbySessionId(o);
		MatchSessionId = CAPI.ovr_GroupPresenceJoinIntent_GetMatchSessionId(o);
	}
}
