using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithLivestreamingVideoStats : Message<LivestreamingVideoStats>
{
	public MessageWithLivestreamingVideoStats(IntPtr c_message)
		: base(c_message)
	{
	}

	public override LivestreamingVideoStats GetLivestreamingVideoStats()
	{
		return base.Data;
	}

	protected override LivestreamingVideoStats GetDataFromMessage(IntPtr c_message)
	{
		return new LivestreamingVideoStats(CAPI.ovr_Message_GetLivestreamingVideoStats(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
