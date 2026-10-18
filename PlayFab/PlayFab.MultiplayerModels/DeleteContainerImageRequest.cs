using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class DeleteContainerImageRequest : PlayFabRequestCommon
{
	public string ImageName;
}
