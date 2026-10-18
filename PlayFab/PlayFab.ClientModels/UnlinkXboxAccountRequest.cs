using System;
using PlayFab.SharedModels;

namespace PlayFab.ClientModels;

[Serializable]
public class UnlinkXboxAccountRequest : PlayFabRequestCommon
{
	[Obsolete("No longer available", true)]
	public string XboxToken;
}
