using System;
using PlayFab.SharedModels;

namespace PlayFab.ClientModels;

[Serializable]
public class ReportAdActivityRequest : PlayFabRequestCommon
{
	public AdActivity Activity;

	public string PlacementId;

	public string RewardId;
}
