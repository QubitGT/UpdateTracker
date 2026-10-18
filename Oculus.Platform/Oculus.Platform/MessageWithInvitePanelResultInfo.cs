using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithInvitePanelResultInfo : Message<InvitePanelResultInfo>
{
	public MessageWithInvitePanelResultInfo(IntPtr c_message)
		: base(c_message)
	{
	}

	public override InvitePanelResultInfo GetInvitePanelResultInfo()
	{
		return base.Data;
	}

	protected override InvitePanelResultInfo GetDataFromMessage(IntPtr c_message)
	{
		return new InvitePanelResultInfo(CAPI.ovr_Message_GetInvitePanelResultInfo(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
