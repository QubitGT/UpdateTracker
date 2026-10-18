using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class ListServerBackfillTicketsForPlayerRequest : PlayFabRequestCommon
{
	public EntityKey Entity;

	public string QueueName;
}
