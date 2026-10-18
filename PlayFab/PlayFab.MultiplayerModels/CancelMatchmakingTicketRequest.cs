using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class CancelMatchmakingTicketRequest : PlayFabRequestCommon
{
	public string QueueName;

	public string TicketId;
}
