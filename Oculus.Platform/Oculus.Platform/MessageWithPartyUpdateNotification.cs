using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithPartyUpdateNotification : Message<PartyUpdateNotification>
{
	public MessageWithPartyUpdateNotification(IntPtr c_message)
		: base(c_message)
	{
	}

	public override PartyUpdateNotification GetPartyUpdateNotification()
	{
		return base.Data;
	}

	protected override PartyUpdateNotification GetDataFromMessage(IntPtr c_message)
	{
		return new PartyUpdateNotification(CAPI.ovr_Message_GetPartyUpdateNotification(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
