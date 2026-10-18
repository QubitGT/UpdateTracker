using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithLaunchInvitePanelFlowResult : Message<LaunchInvitePanelFlowResult>
{
	public MessageWithLaunchInvitePanelFlowResult(IntPtr c_message)
		: base(c_message)
	{
	}

	public override LaunchInvitePanelFlowResult GetLaunchInvitePanelFlowResult()
	{
		return base.Data;
	}

	protected override LaunchInvitePanelFlowResult GetDataFromMessage(IntPtr c_message)
	{
		return new LaunchInvitePanelFlowResult(CAPI.ovr_Message_GetLaunchInvitePanelFlowResult(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
