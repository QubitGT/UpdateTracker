using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithNetSyncVoipAttenuationValueList : Message<NetSyncVoipAttenuationValueList>
{
	public MessageWithNetSyncVoipAttenuationValueList(IntPtr c_message)
		: base(c_message)
	{
	}

	public override NetSyncVoipAttenuationValueList GetNetSyncVoipAttenuationValueList()
	{
		return base.Data;
	}

	protected override NetSyncVoipAttenuationValueList GetDataFromMessage(IntPtr c_message)
	{
		return new NetSyncVoipAttenuationValueList(CAPI.ovr_Message_GetNetSyncVoipAttenuationValueArray(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
