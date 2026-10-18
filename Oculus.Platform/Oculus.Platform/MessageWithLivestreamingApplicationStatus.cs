using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithLivestreamingApplicationStatus : Message<LivestreamingApplicationStatus>
{
	public MessageWithLivestreamingApplicationStatus(IntPtr c_message)
		: base(c_message)
	{
	}

	public override LivestreamingApplicationStatus GetLivestreamingApplicationStatus()
	{
		return base.Data;
	}

	protected override LivestreamingApplicationStatus GetDataFromMessage(IntPtr c_message)
	{
		return new LivestreamingApplicationStatus(CAPI.ovr_Message_GetLivestreamingApplicationStatus(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
