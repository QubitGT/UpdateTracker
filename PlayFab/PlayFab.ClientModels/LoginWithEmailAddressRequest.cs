using System;
using PlayFab.SharedModels;

namespace PlayFab.ClientModels;

[Serializable]
public class LoginWithEmailAddressRequest : PlayFabRequestCommon
{
	public string Email;

	public GetPlayerCombinedInfoRequestParams InfoRequestParameters;

	public string Password;

	public string TitleId;
}
