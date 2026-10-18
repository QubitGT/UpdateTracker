using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class CancelServerBackfillTicketRequest : PlayFabRequestCommon
{
	public string QueueName;

	public string TicketId;
}
