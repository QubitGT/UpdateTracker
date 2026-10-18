using System;
using PlayFab.SharedModels;

namespace PlayFab.ExperimentationModels;

[Serializable]
public class GetLatestScorecardRequest : PlayFabRequestCommon
{
	public string ExperimentId;
}
