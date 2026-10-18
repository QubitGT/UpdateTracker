using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithLaunchReportFlowResult : Message<LaunchReportFlowResult>
{
	public MessageWithLaunchReportFlowResult(IntPtr c_message)
		: base(c_message)
	{
	}

	public override LaunchReportFlowResult GetLaunchReportFlowResult()
	{
		return base.Data;
	}

	protected override LaunchReportFlowResult GetDataFromMessage(IntPtr c_message)
	{
		return new LaunchReportFlowResult(CAPI.ovr_Message_GetLaunchReportFlowResult(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
