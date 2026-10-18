using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithBlockedUserList : Message<BlockedUserList>
{
	public MessageWithBlockedUserList(IntPtr c_message)
		: base(c_message)
	{
	}

	public override BlockedUserList GetBlockedUserList()
	{
		return base.Data;
	}

	protected override BlockedUserList GetDataFromMessage(IntPtr c_message)
	{
		return new BlockedUserList(CAPI.ovr_Message_GetBlockedUserArray(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
