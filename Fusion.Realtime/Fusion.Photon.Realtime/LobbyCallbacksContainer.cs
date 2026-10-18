using System.Collections.Generic;

namespace Fusion.Photon.Realtime;

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
			ILobbyCallbacks current = enumerator.Current;
			current.OnJoinedLobby();
		}
	}

	public void OnLeftLobby()
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			ILobbyCallbacks current = enumerator.Current;
			current.OnLeftLobby();
		}
	}

	public void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			ILobbyCallbacks current = enumerator.Current;
			current.OnRoomListUpdate(roomList);
		}
	}

	public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			ILobbyCallbacks current = enumerator.Current;
			current.OnLobbyStatisticsUpdate(lobbyStatistics);
		}
	}
}
