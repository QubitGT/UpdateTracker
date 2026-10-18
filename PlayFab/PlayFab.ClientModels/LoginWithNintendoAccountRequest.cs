using System;
using PlayFab.SharedModels;

namespace PlayFab.ClientModels;

[Serializable]
public class LoginWithNintendoAccountRequest : PlayFabRequestCommon
{
	public bool? CreateAccount;

	public string EncryptedRequest;

	public string IdentityToken;

	public GetPlayerCombinedInfoRequestParams InfoRequestParameters;

	public string PlayerSecret;

	public string TitleId;
}
