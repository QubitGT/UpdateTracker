using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class GetServerBackfillTicketRequest : PlayFabRequestCommon
{
	public bool EscapeObject;

	public string QueueName;

	public string TicketId;
}
