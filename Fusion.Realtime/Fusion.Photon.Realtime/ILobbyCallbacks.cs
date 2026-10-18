using System.Collections.Generic;

namespace Fusion.Photon.Realtime;

internal interface ILobbyCallbacks
{
	void OnJoinedLobby();

	void OnLeftLobby();

	void OnRoomListUpdate(List<RoomInfo> roomList);

	void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics);
}
