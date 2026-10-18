using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace Photon.Realtime;

internal class InRoomCallbacksContainer : List<IInRoomCallbacks>, IInRoomCallbacks
{
	private readonly LoadBalancingClient client;

	public InRoomCallbacksContainer(LoadBalancingClient client)
	{
		this.client = client;
	}

	public void OnPlayerEnteredRoom(Player newPlayer)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnPlayerEnteredRoom(newPlayer);
		}
	}

	public void OnPlayerLeftRoom(Player otherPlayer)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnPlayerLeftRoom(otherPlayer);
		}
	}

	public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnRoomPropertiesUpdate(propertiesThatChanged);
		}
	}

	public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProp)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnPlayerPropertiesUpdate(targetPlayer, changedProp);
		}
	}

	public void OnMasterClientSwitched(Player newMasterClient)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnMasterClientSwitched(newMasterClient);
		}
	}
}
