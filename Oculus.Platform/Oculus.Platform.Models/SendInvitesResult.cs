using System;

namespace Oculus.Platform.Models;

public class SendInvitesResult
{
	public readonly ApplicationInviteList Invites;

	public SendInvitesResult(IntPtr o)
	{
		Invites = new ApplicationInviteList(CAPI.ovr_SendInvitesResult_GetInvites(o));
	}
}
