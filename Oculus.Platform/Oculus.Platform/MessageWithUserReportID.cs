using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithUserReportID : Message<UserReportID>
{
	public MessageWithUserReportID(IntPtr c_message)
		: base(c_message)
	{
	}

	public override UserReportID GetUserReportID()
	{
		return base.Data;
	}

	protected override UserReportID GetDataFromMessage(IntPtr c_message)
	{
		return new UserReportID(CAPI.ovr_Message_GetUserReportID(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
