using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class DeleteBuildRegionRequest : PlayFabRequestCommon
{
	public string BuildId;

	public string Region;
}
