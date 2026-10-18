using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithLinkedAccountList : Message<LinkedAccountList>
{
	public MessageWithLinkedAccountList(IntPtr c_message)
		: base(c_message)
	{
	}

	public override LinkedAccountList GetLinkedAccountList()
	{
		return base.Data;
	}

	protected override LinkedAccountList GetDataFromMessage(IntPtr c_message)
	{
		return new LinkedAccountList(CAPI.ovr_Message_GetLinkedAccountArray(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
