using System;
using PlayFab.SharedModels;

namespace PlayFab.ClientModels;

[Serializable]
public class LoginWithTwitchRequest : PlayFabRequestCommon
{
	public string AccessToken;

	public bool? CreateAccount;

	public string EncryptedRequest;

	public GetPlayerCombinedInfoRequestParams InfoRequestParameters;

	public string PlayerSecret;

	public string TitleId;
}
