using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithNetSyncSessionList : Message<NetSyncSessionList>
{
	public MessageWithNetSyncSessionList(IntPtr c_message)
		: base(c_message)
	{
	}

	public override NetSyncSessionList GetNetSyncSessionList()
	{
		return base.Data;
	}

	protected override NetSyncSessionList GetDataFromMessage(IntPtr c_message)
	{
		return new NetSyncSessionList(CAPI.ovr_Message_GetNetSyncSessionArray(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
