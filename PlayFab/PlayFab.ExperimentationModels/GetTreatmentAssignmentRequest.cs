using System;
using PlayFab.SharedModels;

namespace PlayFab.ExperimentationModels;

[Serializable]
public class GetTreatmentAssignmentRequest : PlayFabRequestCommon
{
	public EntityKey Entity;
}
