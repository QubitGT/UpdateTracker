using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithLeaderboardEntryList : Message<LeaderboardEntryList>
{
	public MessageWithLeaderboardEntryList(IntPtr c_message)
		: base(c_message)
	{
	}

	public override LeaderboardEntryList GetLeaderboardEntryList()
	{
		return base.Data;
	}

	protected override LeaderboardEntryList GetDataFromMessage(IntPtr c_message)
	{
		return new LeaderboardEntryList(CAPI.ovr_Message_GetLeaderboardEntryArray(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
