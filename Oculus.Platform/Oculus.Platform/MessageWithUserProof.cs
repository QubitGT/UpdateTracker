using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithUserProof : Message<UserProof>
{
	public MessageWithUserProof(IntPtr c_message)
		: base(c_message)
	{
	}

	public override UserProof GetUserProof()
	{
		return base.Data;
	}

	protected override UserProof GetDataFromMessage(IntPtr c_message)
	{
		return new UserProof(CAPI.ovr_Message_GetUserProof(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
