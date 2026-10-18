using System;
using PlayFab.SharedModels;

namespace PlayFab.InsightsModels;

[Serializable]
public class InsightsSetPerformanceRequest : PlayFabRequestCommon
{
	public int PerformanceLevel;
}
