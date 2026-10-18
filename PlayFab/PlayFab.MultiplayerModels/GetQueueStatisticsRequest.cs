using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class GetQueueStatisticsRequest : PlayFabRequestCommon
{
	public string QueueName;
}
