using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithUserCapabilityList : Message<UserCapabilityList>
{
	public MessageWithUserCapabilityList(IntPtr c_message)
		: base(c_message)
	{
	}

	public override UserCapabilityList GetUserCapabilityList()
	{
		return base.Data;
	}

	protected override UserCapabilityList GetDataFromMessage(IntPtr c_message)
	{
		return new UserCapabilityList(CAPI.ovr_Message_GetUserCapabilityArray(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
