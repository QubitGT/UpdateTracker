using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class ListPartyQosServersRequest : PlayFabRequestCommon
{
	public string Version;
}
