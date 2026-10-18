using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithUser : Message<User>
{
	public MessageWithUser(IntPtr c_message)
		: base(c_message)
	{
	}

	public override User GetUser()
	{
		return base.Data;
	}

	protected override User GetDataFromMessage(IntPtr c_message)
	{
		return new User(CAPI.ovr_Message_GetUser(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
