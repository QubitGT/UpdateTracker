using System.Collections.Generic;

namespace Photon.Realtime;

internal class LobbyCallbacksContainer : List<ILobbyCallbacks>, ILobbyCallbacks
{
	private readonly LoadBalancingClient client;

	public LobbyCallbacksContainer(LoadBalancingClient client)
	{
		this.client = client;
	}

	public void OnJoinedLobby()
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnJoinedLobby();
		}
	}

	public void OnLeftLobby()
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnLeftLobby();
		}
	}

	public void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnRoomListUpdate(roomList);
		}
	}

	public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnLobbyStatisticsUpdate(lobbyStatistics);
		}
	}
}
