using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class UpdateBuildRegionRequest : PlayFabRequestCommon
{
	public string BuildId;

	public BuildRegionParams BuildRegion;
}
