using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class ListVirtualMachineSummariesRequest : PlayFabRequestCommon
{
	public string BuildId;

	public int? PageSize;

	public string Region;

	public string SkipToken;
}
