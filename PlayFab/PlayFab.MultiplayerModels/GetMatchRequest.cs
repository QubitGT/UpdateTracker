using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class GetMatchRequest : PlayFabRequestCommon
{
	public bool EscapeObject;

	public string MatchId;

	public string QueueName;

	public bool ReturnMemberAttributes;
}
