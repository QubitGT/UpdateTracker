using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithChallengeList : Message<ChallengeList>
{
	public MessageWithChallengeList(IntPtr c_message)
		: base(c_message)
	{
	}

	public override ChallengeList GetChallengeList()
	{
		return base.Data;
	}

	protected override ChallengeList GetDataFromMessage(IntPtr c_message)
	{
		return new ChallengeList(CAPI.ovr_Message_GetChallengeArray(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
