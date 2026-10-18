using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class UntagContainerImageRequest : PlayFabRequestCommon
{
	public string ImageName;

	public string Tag;
}
