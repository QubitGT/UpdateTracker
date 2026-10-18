using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithPidList : Message<PidList>
{
	public MessageWithPidList(IntPtr c_message)
		: base(c_message)
	{
	}

	public override PidList GetPidList()
	{
		return base.Data;
	}

	protected override PidList GetDataFromMessage(IntPtr c_message)
	{
		return new PidList(CAPI.ovr_Message_GetPidArray(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
