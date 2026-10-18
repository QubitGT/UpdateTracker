using System;
using PlayFab.SharedModels;

namespace PlayFab.ClientModels;

[Serializable]
public class UnlinkOpenIdConnectRequest : PlayFabRequestCommon
{
	public string ConnectionId;
}
