using System.Collections.Generic;

namespace Fusion.Photon.Realtime;

internal class MatchMakingCallbacksContainer : List<IMatchmakingCallbacks>, IMatchmakingCallbacks
{
	private readonly LoadBalancingClient client;

	public MatchMakingCallbacksContainer(LoadBalancingClient client)
	{
		this.client = client;
	}

	public void OnCreatedRoom()
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			IMatchmakingCallbacks current = enumerator.Current;
			current.OnCreatedRoom();
		}
	}

	public void OnJoinedRoom()
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			IMatchmakingCallbacks current = enumerator.Current;
			current.OnJoinedRoom();
		}
	}

	public void OnCreateRoomFailed(short returnCode, string message)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			IMatchmakingCallbacks current = enumerator.Current;
			current.OnCreateRoomFailed(returnCode, message);
		}
	}

	public void OnJoinRandomFailed(short returnCode, string message)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			IMatchmakingCallbacks current = enumerator.Current;
			current.OnJoinRandomFailed(returnCode, message);
		}
	}

	public void OnJoinRoomFailed(short returnCode, string message)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			IMatchmakingCallbacks current = enumerator.Current;
			current.OnJoinRoomFailed(returnCode, message);
		}
	}

	public void OnLeftRoom()
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			IMatchmakingCallbacks current = enumerator.Current;
			current.OnLeftRoom();
		}
	}

	public void OnFriendListUpdate(List<FriendInfo> friendList)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			IMatchmakingCallbacks current = enumerator.Current;
			current.OnFriendListUpdate(friendList);
		}
	}
}
