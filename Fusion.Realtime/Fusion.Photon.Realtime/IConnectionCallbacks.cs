using System.Collections.Generic;

namespace Fusion.Photon.Realtime;

internal interface IConnectionCallbacks
{
	void OnConnected();

	void OnConnectedToMaster();

	void OnDisconnected(DisconnectCause cause);

	void OnRegionListReceived(RegionHandler regionHandler);

	void OnCustomAuthenticationResponse(Dictionary<string, object> data);

	void OnCustomAuthenticationFailed(string debugMessage);
}
