using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class SetMatchmakingQueueRequest : PlayFabRequestCommon
{
	public MatchmakingQueueConfig MatchmakingQueue;
}
