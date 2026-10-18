using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithAbuseReportRecording : Message<AbuseReportRecording>
{
	public MessageWithAbuseReportRecording(IntPtr c_message)
		: base(c_message)
	{
	}

	public override AbuseReportRecording GetAbuseReportRecording()
	{
		return base.Data;
	}

	protected override AbuseReportRecording GetDataFromMessage(IntPtr c_message)
	{
		return new AbuseReportRecording(CAPI.ovr_Message_GetAbuseReportRecording(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
