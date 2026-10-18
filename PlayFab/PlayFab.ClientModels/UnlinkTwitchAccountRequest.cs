using System;
using PlayFab.SharedModels;

namespace PlayFab.ClientModels;

[Serializable]
public class UnlinkTwitchAccountRequest : PlayFabRequestCommon
{
	public string AccessToken;
}
