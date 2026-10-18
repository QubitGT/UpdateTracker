using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithNetSyncConnection : Message<NetSyncConnection>
{
	public MessageWithNetSyncConnection(IntPtr c_message)
		: base(c_message)
	{
	}

	public override NetSyncConnection GetNetSyncConnection()
	{
		return base.Data;
	}

	protected override NetSyncConnection GetDataFromMessage(IntPtr c_message)
	{
		return new NetSyncConnection(CAPI.ovr_Message_GetNetSyncConnection(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
