using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class ShutdownMultiplayerServerRequest : PlayFabRequestCommon
{
	public string BuildId;

	public string Region;

	public string SessionId;
}
