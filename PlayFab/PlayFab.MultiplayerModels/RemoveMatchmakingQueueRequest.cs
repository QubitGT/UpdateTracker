using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class RemoveMatchmakingQueueRequest : PlayFabRequestCommon
{
	public string QueueName;
}
