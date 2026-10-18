using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithLivestreamingStartResult : Message<LivestreamingStartResult>
{
	public MessageWithLivestreamingStartResult(IntPtr c_message)
		: base(c_message)
	{
	}

	public override LivestreamingStartResult GetLivestreamingStartResult()
	{
		return base.Data;
	}

	protected override LivestreamingStartResult GetDataFromMessage(IntPtr c_message)
	{
		return new LivestreamingStartResult(CAPI.ovr_Message_GetLivestreamingStartResult(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
