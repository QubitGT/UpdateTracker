using System;

namespace Meta.XR.MultiplayerBlocks.Shared;

[Serializable]
internal struct MatchInfo
{
	internal string RoomId;

	internal string RoomPassword;

	internal string Extra;

	public MatchInfo(string roomId, string roomPassword, string extra = "")
	{
		RoomId = roomId;
		RoomPassword = roomPassword;
		Extra = extra;
	}
}
