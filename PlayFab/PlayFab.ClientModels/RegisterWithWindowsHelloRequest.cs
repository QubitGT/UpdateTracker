using System;
using PlayFab.SharedModels;

namespace PlayFab.ClientModels;

[Serializable]
public class RegisterWithWindowsHelloRequest : PlayFabRequestCommon
{
	public string DeviceName;

	public string EncryptedRequest;

	public GetPlayerCombinedInfoRequestParams InfoRequestParameters;

	public string PlayerSecret;

	public string PublicKey;

	public string TitleId;

	public string UserName;
}
