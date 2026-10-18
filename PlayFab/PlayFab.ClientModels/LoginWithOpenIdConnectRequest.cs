using System;
using PlayFab.SharedModels;

namespace PlayFab.ClientModels;

[Serializable]
public class LoginWithOpenIdConnectRequest : PlayFabRequestCommon
{
	public string ConnectionId;

	public bool? CreateAccount;

	public string EncryptedRequest;

	public string IdToken;

	public GetPlayerCombinedInfoRequestParams InfoRequestParameters;

	public string PlayerSecret;

	public string TitleId;
}
