using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithDestinationList : Message<DestinationList>
{
	public MessageWithDestinationList(IntPtr c_message)
		: base(c_message)
	{
	}

	public override DestinationList GetDestinationList()
	{
		return base.Data;
	}

	protected override DestinationList GetDataFromMessage(IntPtr c_message)
	{
		return new DestinationList(CAPI.ovr_Message_GetDestinationArray(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
