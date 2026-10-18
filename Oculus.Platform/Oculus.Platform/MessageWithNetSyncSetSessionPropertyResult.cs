using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithNetSyncSetSessionPropertyResult : Message<NetSyncSetSessionPropertyResult>
{
	public MessageWithNetSyncSetSessionPropertyResult(IntPtr c_message)
		: base(c_message)
	{
	}

	public override NetSyncSetSessionPropertyResult GetNetSyncSetSessionPropertyResult()
	{
		return base.Data;
	}

	protected override NetSyncSetSessionPropertyResult GetDataFromMessage(IntPtr c_message)
	{
		return new NetSyncSetSessionPropertyResult(CAPI.ovr_Message_GetNetSyncSetSessionPropertyResult(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
