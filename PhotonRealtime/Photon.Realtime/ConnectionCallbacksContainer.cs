using System.Collections.Generic;

namespace Photon.Realtime;

public class ConnectionCallbacksContainer : List<IConnectionCallbacks>, IConnectionCallbacks
{
	private readonly LoadBalancingClient client;

	public ConnectionCallbacksContainer(LoadBalancingClient client)
	{
		this.client = client;
	}

	public void OnConnected()
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnConnected();
		}
	}

	public void OnConnectedToMaster()
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnConnectedToMaster();
		}
	}

	public void OnRegionListReceived(RegionHandler regionHandler)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnRegionListReceived(regionHandler);
		}
	}

	public void OnDisconnected(DisconnectCause cause)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnDisconnected(cause);
		}
	}

	public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnCustomAuthenticationResponse(data);
		}
	}

	public void OnCustomAuthenticationFailed(string debugMessage)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnCustomAuthenticationFailed(debugMessage);
		}
	}
}
