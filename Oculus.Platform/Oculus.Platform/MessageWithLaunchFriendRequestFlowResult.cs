using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithLaunchFriendRequestFlowResult : Message<LaunchFriendRequestFlowResult>
{
	public MessageWithLaunchFriendRequestFlowResult(IntPtr c_message)
		: base(c_message)
	{
	}

	public override LaunchFriendRequestFlowResult GetLaunchFriendRequestFlowResult()
	{
		return base.Data;
	}

	protected override LaunchFriendRequestFlowResult GetDataFromMessage(IntPtr c_message)
	{
		return new LaunchFriendRequestFlowResult(CAPI.ovr_Message_GetLaunchFriendRequestFlowResult(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
