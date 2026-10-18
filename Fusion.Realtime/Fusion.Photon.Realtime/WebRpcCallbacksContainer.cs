using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace Fusion.Photon.Realtime;

internal class WebRpcCallbacksContainer : List<IWebRpcCallback>, IWebRpcCallback
{
	private LoadBalancingClient client;

	public WebRpcCallbacksContainer(LoadBalancingClient client)
	{
		this.client = client;
	}

	public void OnWebRpcResponse(OperationResponse response)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			IWebRpcCallback current = enumerator.Current;
			current.OnWebRpcResponse(response);
		}
	}
}
