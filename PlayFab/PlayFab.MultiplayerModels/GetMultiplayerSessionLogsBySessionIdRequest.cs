using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class GetMultiplayerSessionLogsBySessionIdRequest : PlayFabRequestCommon
{
	public string SessionId;
}
