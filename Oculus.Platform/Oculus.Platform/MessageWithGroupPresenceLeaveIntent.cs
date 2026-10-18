using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithGroupPresenceLeaveIntent : Message<GroupPresenceLeaveIntent>
{
	public MessageWithGroupPresenceLeaveIntent(IntPtr c_message)
		: base(c_message)
	{
	}

	public override GroupPresenceLeaveIntent GetGroupPresenceLeaveIntent()
	{
		return base.Data;
	}

	protected override GroupPresenceLeaveIntent GetDataFromMessage(IntPtr c_message)
	{
		return new GroupPresenceLeaveIntent(CAPI.ovr_Message_GetGroupPresenceLeaveIntent(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
