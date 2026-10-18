using System;
using System.Collections.Generic;

namespace Fusion.Photon.Realtime.Async;

internal class PhotonMatchmakingCallbacks
{
	public Action<List<FriendInfo>> FriendListUpdate;

	public Action JoinedRoom;

	public Action CreatedRoom;

	public Action<short, string> JoinRoomFailed;

	public Action<short, string> JoinRoomRandomFailed;

	public Action<short, string> CreateRoomFailed;

	public Action LeftRoom;
}
