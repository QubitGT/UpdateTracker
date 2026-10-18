using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithShareMediaResult : Message<ShareMediaResult>
{
	public MessageWithShareMediaResult(IntPtr c_message)
		: base(c_message)
	{
	}

	public override ShareMediaResult GetShareMediaResult()
	{
		return base.Data;
	}

	protected override ShareMediaResult GetDataFromMessage(IntPtr c_message)
	{
		return new ShareMediaResult(CAPI.ovr_Message_GetShareMediaResult(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
