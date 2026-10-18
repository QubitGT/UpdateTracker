using System;

namespace Oculus.Platform.Models;

public class LaunchInvitePanelFlowResult
{
	public readonly UserList InvitedUsers;

	public LaunchInvitePanelFlowResult(IntPtr o)
	{
		InvitedUsers = new UserList(CAPI.ovr_LaunchInvitePanelFlowResult_GetInvitedUsers(o));
	}
}
