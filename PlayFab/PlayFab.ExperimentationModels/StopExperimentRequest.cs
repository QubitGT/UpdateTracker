using System;
using PlayFab.SharedModels;

namespace PlayFab.ExperimentationModels;

[Serializable]
public class StopExperimentRequest : PlayFabRequestCommon
{
	public string ExperimentId;
}
