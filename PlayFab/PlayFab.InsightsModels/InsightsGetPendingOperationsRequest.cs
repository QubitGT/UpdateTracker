using System;
using PlayFab.SharedModels;

namespace PlayFab.InsightsModels;

[Serializable]
public class InsightsGetPendingOperationsRequest : PlayFabRequestCommon
{
	public string OperationType;
}
