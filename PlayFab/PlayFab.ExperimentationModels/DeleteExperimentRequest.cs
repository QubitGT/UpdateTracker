using System;
using PlayFab.SharedModels;

namespace PlayFab.ExperimentationModels;

[Serializable]
public class DeleteExperimentRequest : PlayFabRequestCommon
{
	public string ExperimentId;
}
