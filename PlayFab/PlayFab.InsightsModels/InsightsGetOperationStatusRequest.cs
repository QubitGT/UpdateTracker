using System;
using PlayFab.SharedModels;

namespace PlayFab.InsightsModels;

[Serializable]
public class InsightsGetOperationStatusRequest : PlayFabRequestCommon
{
	public string OperationId;
}
