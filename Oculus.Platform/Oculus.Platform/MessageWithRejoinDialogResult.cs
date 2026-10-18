using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithRejoinDialogResult : Message<RejoinDialogResult>
{
	public MessageWithRejoinDialogResult(IntPtr c_message)
		: base(c_message)
	{
	}

	public override RejoinDialogResult GetRejoinDialogResult()
	{
		return base.Data;
	}

	protected override RejoinDialogResult GetDataFromMessage(IntPtr c_message)
	{
		return new RejoinDialogResult(CAPI.ovr_Message_GetRejoinDialogResult(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
