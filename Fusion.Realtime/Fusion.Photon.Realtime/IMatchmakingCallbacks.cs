using System.Collections.Generic;

namespace Fusion.Photon.Realtime;

internal interface IMatchmakingCallbacks
{
	void OnFriendListUpdate(List<FriendInfo> friendList);

	void OnCreatedRoom();

	void OnCreateRoomFailed(short returnCode, string message);

	void OnJoinedRoom();

	void OnJoinRoomFailed(short returnCode, string message);

	void OnJoinRandomFailed(short returnCode, string message);

	void OnLeftRoom();
}
