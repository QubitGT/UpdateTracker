using System;
using System.Collections.Generic;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class CreateBuildAliasRequest : PlayFabRequestCommon
{
	public string AliasName;

	public List<BuildSelectionCriterion> BuildSelectionCriteria;
}
