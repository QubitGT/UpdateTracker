using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class GetRemoteLoginEndpointRequest : PlayFabRequestCommon
{
	public string BuildId;

	public string Region;

	public string VmId;
}
