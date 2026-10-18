using System;
using System.Collections.Generic;

namespace Fusion.Photon.Realtime.Async;

internal class PhotonConnectionCallbacks
{
	public Action ConnectedToMaster;

	public Action ConnectedToNameServer;

	public Action<RegionHandler> RegionListReceived;

	public Action<DisconnectCause> Disconnected;

	public Action<string> CustomAuthenticationFailed;

	public Action<Dictionary<string, object>> CustomAuthenticationResponse;
}
