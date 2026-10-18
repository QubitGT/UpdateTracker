using System;
using PlayFab.SharedModels;

namespace PlayFab.InsightsModels;

[Serializable]
public class InsightsSetStorageRetentionRequest : PlayFabRequestCommon
{
	public int RetentionDays;
}
