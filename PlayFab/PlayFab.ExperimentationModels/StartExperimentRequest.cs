using System;
using PlayFab.SharedModels;

namespace PlayFab.ExperimentationModels;

[Serializable]
public class StartExperimentRequest : PlayFabRequestCommon
{
	public string ExperimentId;
}
