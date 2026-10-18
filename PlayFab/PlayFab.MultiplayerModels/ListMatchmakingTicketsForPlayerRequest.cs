using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class ListMatchmakingTicketsForPlayerRequest : PlayFabRequestCommon
{
	public EntityKey Entity;

	public string QueueName;
}
