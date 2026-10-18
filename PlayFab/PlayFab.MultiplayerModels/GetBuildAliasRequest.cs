using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class GetBuildAliasRequest : PlayFabRequestCommon
{
	public string AliasId;
}
