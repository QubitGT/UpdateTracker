using System;
using PlayFab.SharedModels;

namespace PlayFab.AuthenticationModels;

[Serializable]
public class ValidateEntityTokenRequest : PlayFabRequestCommon
{
	public string EntityToken;
}
