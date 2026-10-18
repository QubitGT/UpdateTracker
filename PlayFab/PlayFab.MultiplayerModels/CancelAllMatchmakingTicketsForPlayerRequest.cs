using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class CancelAllMatchmakingTicketsForPlayerRequest : PlayFabRequestCommon
{
	public EntityKey Entity;

	public string QueueName;
}
