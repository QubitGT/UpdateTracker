using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class GetMatchmakingQueueRequest : PlayFabRequestCommon
{
	public string QueueName;
}
