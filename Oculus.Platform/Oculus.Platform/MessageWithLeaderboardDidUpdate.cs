using System;

namespace Oculus.Platform;

public class MessageWithLeaderboardDidUpdate : Message<bool>
{
	public MessageWithLeaderboardDidUpdate(IntPtr c_message)
		: base(c_message)
	{
	}

	public override bool GetLeaderboardDidUpdate()
	{
		return base.Data;
	}

	protected override bool GetDataFromMessage(IntPtr c_message)
	{
		return CAPI.ovr_LeaderboardUpdateStatus_GetDidUpdate(CAPI.ovr_Message_GetLeaderboardUpdateStatus(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
