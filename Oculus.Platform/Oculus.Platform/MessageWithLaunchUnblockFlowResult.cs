using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithLaunchUnblockFlowResult : Message<LaunchUnblockFlowResult>
{
	public MessageWithLaunchUnblockFlowResult(IntPtr c_message)
		: base(c_message)
	{
	}

	public override LaunchUnblockFlowResult GetLaunchUnblockFlowResult()
	{
		return base.Data;
	}

	protected override LaunchUnblockFlowResult GetDataFromMessage(IntPtr c_message)
	{
		return new LaunchUnblockFlowResult(CAPI.ovr_Message_GetLaunchUnblockFlowResult(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
