using System;

namespace Oculus.Platform.Models;

public class InvitePanelResultInfo
{
	public readonly bool InvitesSent;

	public InvitePanelResultInfo(IntPtr o)
	{
		InvitesSent = CAPI.ovr_InvitePanelResultInfo_GetInvitesSent(o);
	}
}
