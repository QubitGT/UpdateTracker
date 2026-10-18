using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class DeleteBuildAliasRequest : PlayFabRequestCommon
{
	public string AliasId;
}
