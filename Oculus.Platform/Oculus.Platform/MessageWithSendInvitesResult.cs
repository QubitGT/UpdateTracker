using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithSendInvitesResult : Message<SendInvitesResult>
{
	public MessageWithSendInvitesResult(IntPtr c_message)
		: base(c_message)
	{
	}

	public override SendInvitesResult GetSendInvitesResult()
	{
		return base.Data;
	}

	protected override SendInvitesResult GetDataFromMessage(IntPtr c_message)
	{
		return new SendInvitesResult(CAPI.ovr_Message_GetSendInvitesResult(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
