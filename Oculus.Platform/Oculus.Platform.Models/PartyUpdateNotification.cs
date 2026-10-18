using System;

namespace Oculus.Platform.Models;

public class PartyUpdateNotification
{
	public readonly PartyUpdateAction Action;

	public readonly ulong PartyId;

	public readonly ulong SenderId;

	public readonly string UpdateTimestamp;

	public readonly string UserAlias;

	public readonly ulong UserId;

	public readonly string UserName;

	public PartyUpdateNotification(IntPtr o)
	{
		Action = CAPI.ovr_PartyUpdateNotification_GetAction(o);
		PartyId = CAPI.ovr_PartyUpdateNotification_GetPartyId(o);
		SenderId = CAPI.ovr_PartyUpdateNotification_GetSenderId(o);
		UpdateTimestamp = CAPI.ovr_PartyUpdateNotification_GetUpdateTimestamp(o);
		UserAlias = CAPI.ovr_PartyUpdateNotification_GetUserAlias(o);
		UserId = CAPI.ovr_PartyUpdateNotification_GetUserId(o);
		UserName = CAPI.ovr_PartyUpdateNotification_GetUserName(o);
	}
}
