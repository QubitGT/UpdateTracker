using System;
using PlayFab.SharedModels;

namespace PlayFab.ClientModels;

[Serializable]
public class LinkAppleRequest : PlayFabRequestCommon
{
	public bool? ForceLink;

	public string IdentityToken;
}
