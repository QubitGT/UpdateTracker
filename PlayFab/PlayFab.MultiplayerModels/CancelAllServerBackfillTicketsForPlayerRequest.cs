using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class CancelAllServerBackfillTicketsForPlayerRequest : PlayFabRequestCommon
{
	public EntityKey Entity;

	public string QueueName;
}
