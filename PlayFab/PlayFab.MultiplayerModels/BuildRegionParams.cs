using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class BuildRegionParams : PlayFabBaseModel
{
	public DynamicStandbySettings DynamicStandbySettings;

	public int MaxServers;

	public string Region;

	public int StandbyServers;
}
