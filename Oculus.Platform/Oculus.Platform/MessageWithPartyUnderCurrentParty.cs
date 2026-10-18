using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithPartyUnderCurrentParty : Message<Party>
{
	public MessageWithPartyUnderCurrentParty(IntPtr c_message)
		: base(c_message)
	{
	}

	public override Party GetParty()
	{
		return base.Data;
	}

	protected override Party GetDataFromMessage(IntPtr c_message)
	{
		return new Party(CAPI.ovr_Message_GetParty(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
