using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class GetMultiplayerServerLogsRequest : PlayFabRequestCommon
{
	public string ServerId;
}
