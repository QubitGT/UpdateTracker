using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithChallenge : Message<Challenge>
{
	public MessageWithChallenge(IntPtr c_message)
		: base(c_message)
	{
	}

	public override Challenge GetChallenge()
	{
		return base.Data;
	}

	protected override Challenge GetDataFromMessage(IntPtr c_message)
	{
		return new Challenge(CAPI.ovr_Message_GetChallenge(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
