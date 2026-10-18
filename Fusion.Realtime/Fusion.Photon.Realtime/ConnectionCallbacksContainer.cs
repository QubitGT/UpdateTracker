using System.Collections.Generic;

namespace Fusion.Photon.Realtime;

internal class ConnectionCallbacksContainer : List<IConnectionCallbacks>, IConnectionCallbacks
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
			IConnectionCallbacks current = enumerator.Current;
			current.OnConnected();
		}
	}

	public void OnConnectedToMaster()
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			IConnectionCallbacks current = enumerator.Current;
			current.OnConnectedToMaster();
		}
	}

	public void OnRegionListReceived(RegionHandler regionHandler)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			IConnectionCallbacks current = enumerator.Current;
			current.OnRegionListReceived(regionHandler);
		}
	}

	public void OnDisconnected(DisconnectCause cause)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			IConnectionCallbacks current = enumerator.Current;
			current.OnDisconnected(cause);
		}
	}

	public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			IConnectionCallbacks current = enumerator.Current;
			current.OnCustomAuthenticationResponse(data);
		}
	}

	public void OnCustomAuthenticationFailed(string debugMessage)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			IConnectionCallbacks current = enumerator.Current;
			current.OnCustomAuthenticationFailed(debugMessage);
		}
	}
}
