using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithLeaderboardList : Message<LeaderboardList>
{
	public MessageWithLeaderboardList(IntPtr c_message)
		: base(c_message)
	{
	}

	public override LeaderboardList GetLeaderboardList()
	{
		return base.Data;
	}

	protected override LeaderboardList GetDataFromMessage(IntPtr c_message)
	{
		return new LeaderboardList(CAPI.ovr_Message_GetLeaderboardArray(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
